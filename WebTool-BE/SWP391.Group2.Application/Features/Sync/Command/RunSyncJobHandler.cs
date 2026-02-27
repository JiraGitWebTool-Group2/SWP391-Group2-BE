using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Domain.Entities;
using SWP391.Group2.Application.Abstractions.GitHub;

namespace SWP391.Group2.Application.Features.Sync.Command
{
    public class RunSyncJobHandler : IRequestHandler<RunSyncJobCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IGitHubClient _gitHub;

        public RunSyncJobHandler(IApplicationDbContext db, IGitHubClient gitHub)
        {
            _db = db;
            _gitHub = gitHub;
        }

        public async Task Handle(RunSyncJobCommand request, CancellationToken ct)
        {
            var run = await _db.SyncRuns.FirstOrDefaultAsync(x => x.SyncRunId == request.SyncRunId, ct);
            if (run is null) return;

            if (run.RunStatus != "RUNNING") return;

            bool jiraSelected = run.IncludeJira;
            bool githubSelected = run.IncludeGithub;

            if (!jiraSelected && !githubSelected)
            {
                run.RunStatus = "FAILED";
                run.FinishedAt = DateTime.UtcNow;
                run.Notes = "No source selected.";
                await _db.SaveChangesAsync(ct);
                return;
            }

            // ====== Fake flags để test ======
            const bool FORCE_JIRA_FAIL = false;
            const bool FORCE_GITHUB_FAIL = false;
            // ===============================

            // Load integration configs theo project_id
            var configs = await _db.ProjectIntegrations.AsNoTracking()
                .Where(x => x.ProjectId == run.ProjectId)
                .ToListAsync(ct);

            var jiraCfg = configs.FirstOrDefault(x => x.Provider == "JIRA");
            var githubCfg = configs.FirstOrDefault(x => x.Provider == "GITHUB");

            bool jiraOk = !jiraSelected;   // nếu không chọn -> N/A
            bool githubOk = !githubSelected;

            string jiraNote = jiraSelected ? "SKIPPED" : "N/A";
            string githubNote = githubSelected ? "SKIPPED" : "N/A";

            // ---------- JIRA ----------
            if (jiraSelected)
            {
                // Validate config trước
                if (jiraCfg is null)
                {
                    jiraOk = false;
                    jiraNote = "FAILED - Missing Jira integration config.";
                }
                else if (string.IsNullOrWhiteSpace(jiraCfg.BaseUrl) || string.IsNullOrWhiteSpace(jiraCfg.ProjectKey) || string.IsNullOrWhiteSpace(jiraCfg.TokenEncrypted))
                {
                    jiraOk = false;
                    jiraNote = "FAILED - Jira config incomplete (baseUrl/projectKey/token).";
                }
                else
                {
                    try
                    {
                        await Task.Delay(800, ct); // fake call

                        if (FORCE_JIRA_FAIL) throw new Exception("Fake Jira connection failed.");

                        // TODO: sau này: gọi Jira API thật bằng jiraCfg
                        jiraOk = true;
                        jiraNote = "SUCCESS (fake)";
                    }
                    catch (Exception ex)
                    {
                        jiraOk = false;
                        jiraNote = $"FAILED - {TrimMsg(ex.Message)}";
                    }
                }
            }

            // ---------- GITHUB ----------
            if (githubSelected)
            {
                if (githubCfg is null)
                {
                    githubOk = false;
                    githubNote = "FAILED - Missing GitHub integration config.";
                }
                else if (string.IsNullOrWhiteSpace(githubCfg.Org) || string.IsNullOrWhiteSpace(githubCfg.TokenEncrypted))
                {
                    githubOk = false;
                    githubNote = "FAILED - GitHub config incomplete (org/token).";
                }
                else
                {
                    try
                    {
                        var toUtc = DateTime.UtcNow;
                        var fromUtc = toUtc.AddDays(-7);

                        var repos = await _db.Repositories.AsNoTracking()
                            .Where(r => r.ProjectId == run.ProjectId)
                            .Select(r => new { r.RepoId, r.RepoName })
                            .ToListAsync(ct);

                        if (repos.Count == 0)
                        {
                            githubOk = true;
                            githubNote = "SUCCESS - No repositories configured.";
                        }
                        else
                        {
                            int inserted = 0;
                            int skipped = 0;

                            // Collect commit IDs that belong to THIS sync window (for snapshot)
                            var commitIdsForSnapshot = new HashSet<int>();

                            foreach (var repo in repos)
                            {
                                var commits = await _gitHub.GetCommitsAsync(
                                    githubCfg.Org!,
                                    repo.RepoName,
                                    fromUtc,
                                    toUtc,
                                    githubCfg.TokenEncrypted!,
                                    ct
                                );

                                foreach (var c in commits)
                                {
                                    // Check tồn tại theo (repo_id, sha)
                                    var existingCommitId = await _db.GitHubCommits
                                        .Where(x => x.RepoId == repo.RepoId && x.CommitHash == c.Sha)
                                        .Select(x => (int?)x.CommitId)
                                        .FirstOrDefaultAsync(ct);

                                    if (existingCommitId.HasValue)
                                    {
                                        skipped++;
                                        commitIdsForSnapshot.Add(existingCommitId.Value);
                                        continue;
                                    }

                                    var entity = new GitHubCommit
                                    {
                                        RepoId = repo.RepoId,
                                        UserId = null,
                                        CommitHash = c.Sha,
                                        Message = c.Message.Length > 1000 ? c.Message[..1000] : c.Message,
                                        CommittedAt = c.CommittedAt,
                                        CommitUrl = string.IsNullOrWhiteSpace(c.Url) ? null : c.Url
                                    };

                                    _db.GitHubCommits.Add(entity);
                                    await _db.SaveChangesAsync(ct); // để có commit_id ngay

                                    inserted++;
                                    commitIdsForSnapshot.Add(entity.CommitId);
                                }
                            }

                            // ===== Create Snapshot + link commits =====
                            var snapshot = new Snapshot
                            {
                                SyncRunId = run.SyncRunId,
                                CapturedAt = DateTime.UtcNow,
                                Label = $"GitHub sync {fromUtc:yyyy-MM-dd}..{toUtc:yyyy-MM-dd}"
                            };

                            _db.Snapshots.Add(snapshot);
                            await _db.SaveChangesAsync(ct); // để có snapshot_id

                            foreach (var commitId in commitIdsForSnapshot)
                            {
                                // tránh duplicate nếu job chạy lại
                                var existsLink = await _db.SnapshotCommits
                                    .AnyAsync(x => x.SnapshotId == snapshot.SnapshotId && x.CommitId == commitId, ct);

                                if (!existsLink)
                                {
                                    _db.SnapshotCommits.Add(new SnapshotCommit
                                    {
                                        SnapshotId = snapshot.SnapshotId,
                                        CommitId = commitId
                                    });
                                }
                            }

                            await _db.SaveChangesAsync(ct);

                            githubOk = true;
                            githubNote = $"SUCCESS - Inserted {inserted}, Skipped {skipped} (last 7 days). SnapshotId={snapshot.SnapshotId}";
                        }
                    }
                    catch (Exception ex)
                    {
                        githubOk = false;
                        githubNote = $"FAILED - {TrimMsg(ex.Message)}";
                    }
                }
            }

            run.Notes = $"JIRA: {jiraNote}; GITHUB: {githubNote}";
            run.FinishedAt = DateTime.UtcNow;

            if (jiraSelected && githubSelected)
            {
                run.RunStatus = (jiraOk && githubOk) ? "SUCCESS"
                            : (!jiraOk && !githubOk) ? "FAILED"
                            : "PARTIAL";
            }
            else if (jiraSelected)
            {
                run.RunStatus = jiraOk ? "SUCCESS" : "FAILED";
            }
            else
            {
                run.RunStatus = githubOk ? "SUCCESS" : "FAILED";
            }

            await _db.SaveChangesAsync(ct);
        }

        private static string TrimMsg(string msg)
            => (msg ?? "").Length <= 200 ? (msg ?? "") : (msg ?? "")[..200];
    }
}
