using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Domain.Entities;
using SWP391.Group2.Application.Abstractions.GitHub;
using SWP391.Group2.Application.Abstractions.Jira;

namespace SWP391.Group2.Application.Features.Sync.Command
{
    public class RunSyncJobHandler : IRequestHandler<RunSyncJobCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IGitHubClient _gitHub;
        private readonly IJiraClient _jira; 

        public RunSyncJobHandler(IApplicationDbContext db, IGitHubClient gitHub, IJiraClient jira)
        {
            _db = db;
            _gitHub = gitHub;
            _jira = jira;

        }

        private sealed record GitHubSyncResult(
            bool Ok,
            string Note,
            int? SnapshotId
        );

        private async Task<GitHubSyncResult> SyncGithubAsync(
            SyncRun run,
            ProjectIntegration? githubCfg,
            CancellationToken ct)
        {
            if (githubCfg is null)
            {
                return new GitHubSyncResult(false, "FAILED - Missing GitHub integration config.", null);
            }

            if (string.IsNullOrWhiteSpace(githubCfg.Org) || string.IsNullOrWhiteSpace(githubCfg.TokenEncrypted))
            {
                return new GitHubSyncResult(false, "FAILED - GitHub config incomplete (org/token).", null);
            }

            if (!run.SyncGithubCommits && !run.SyncGithubPullRequests)
            {
                return new GitHubSyncResult(false, "FAILED - No GitHub item selected.", null);
            }

            try
            {
                var repos = await _db.Repositories
                    .Where(r => r.ProjectId == run.ProjectId)
                    .ToListAsync(ct);

                if (repos.Count == 0)
                {
                    return new GitHubSyncResult(true, "OK - No repositories configured.", null);
                }

                var toUtc = run.GithubTo ?? DateTime.UtcNow;
                var fromUtc = run.GithubFrom ?? toUtc.AddDays(-7);

                var githubAccountMap = await _db.ProjectIntegrations
                    .Where(x => x.ProjectId == run.ProjectId
                             && x.Provider == "GITHUB"
                             && x.LinkedAccount != null)
                    .GroupBy(x => x.LinkedAccount!)
                    .Select(g => new
                    {
                        LinkedAccount = g.Key,
                        UserId = g.Select(x => x.CreatedByUserId).FirstOrDefault()
                    })
                    .ToDictionaryAsync(
                        x => x.LinkedAccount,
                        x => (int?)x.UserId,
                        ct);

                int inserted = 0, skipped = 0, repoOk = 0, repoFail = 0;
                int prInserted = 0, prUpdated = 0, prSkipped = 0;

                var repoErrors = new List<string>();
                var commitIdsForSnapshot = new HashSet<int>();
                var pullRequestIdsForSnapshot = new HashSet<int>();

                foreach (var repo in repos)
                {
                    try
                    {
                        if (run.SyncGithubCommits)
                        {
                            var commits = await _gitHub.GetCommitsAsync(
                            githubCfg.Org!,
                            repo.RepoName,
                            fromUtc,
                            toUtc,
                            githubCfg.TokenEncrypted!,
                            ct);

                            //if (commits.Count == 0)
                            //{
                            //    repoOk++;
                            //    continue;
                            //}

                            //var commitShas = commits
                            //    .Select(c => c.Sha)
                            //    .Where(sha => !string.IsNullOrWhiteSpace(sha))
                            //    .Distinct()
                            //    .ToList();

                            //var existingCommits = await _db.GitHubCommits
                            //    .Where(x => x.RepoId == repo.RepoId && commitShas.Contains(x.CommitHash))
                            //    .ToListAsync(ct);

                            //var existingBySha = existingCommits
                            //    .GroupBy(x => x.CommitHash)
                            //    .ToDictionary(g => g.Key, g => g.First());

                            //var newCommitEntities = new List<GitHubCommit>();

                            //foreach (var c in commits)
                            //{
                            //    if (existingBySha.TryGetValue(c.Sha, out var existing))
                            //    {
                            //        skipped++;
                            //        commitIdsForSnapshot.Add(existing.CommitId);
                            //        continue;
                            //    }

                            //    int? userId = null;
                            //    if (!string.IsNullOrWhiteSpace(c.AuthorLogin) &&
                            //        githubAccountMap.TryGetValue(c.AuthorLogin, out var mappedUserId))
                            //    {
                            //        userId = mappedUserId;
                            //    }

                            //    var entity = new GitHubCommit
                            //    {
                            //        RepoId = repo.RepoId,
                            //        UserId = userId,
                            //        CommitHash = c.Sha,
                            //        Message = c.Message.Length > 1000 ? c.Message[..1000] : c.Message,
                            //        CommittedAt = c.CommittedAt,
                            //        CommitUrl = c.Url
                            //    };

                            //    newCommitEntities.Add(entity);
                            //}

                            if (commits.Count > 0)
                            {
                                var commitShas = commits
                                    .Select(c => c.Sha)
                                    .Where(sha => !string.IsNullOrWhiteSpace(sha))
                                    .Distinct()
                                    .ToList();

                                var existingCommits = await _db.GitHubCommits
                                    .Where(x => x.RepoId == repo.RepoId && commitShas.Contains(x.CommitHash))
                                    .ToListAsync(ct);

                                var existingBySha = existingCommits
                                    .GroupBy(x => x.CommitHash)
                                    .ToDictionary(g => g.Key, g => g.First());

                                var newCommitEntities = new List<GitHubCommit>();

                                foreach (var c in commits)
                                {
                                    if (existingBySha.TryGetValue(c.Sha, out var existing))
                                    {
                                        skipped++;
                                        commitIdsForSnapshot.Add(existing.CommitId);
                                        continue;
                                    }

                                    int? userId = null;
                                    if (!string.IsNullOrWhiteSpace(c.AuthorLogin) &&
                                        githubAccountMap.TryGetValue(c.AuthorLogin, out var mappedUserId))
                                    {
                                        userId = mappedUserId;
                                    }

                                    var entity = new GitHubCommit
                                    {
                                        RepoId = repo.RepoId,
                                        UserId = userId,
                                        CommitHash = c.Sha,
                                        Message = c.Message.Length > 1000 ? c.Message[..1000] : c.Message,
                                        CommittedAt = c.CommittedAt,
                                        CommitUrl = c.Url
                                    };

                                    newCommitEntities.Add(entity);
                                }

                                if (newCommitEntities.Count > 0)
                                {
                                    _db.GitHubCommits.AddRange(newCommitEntities);
                                    await _db.SaveChangesAsync(ct);

                                    inserted += newCommitEntities.Count;

                                    foreach (var entity in newCommitEntities)
                                    {
                                        commitIdsForSnapshot.Add(entity.CommitId);
                                    }
                                }
                            }
                        }

                        if (run.SyncGithubPullRequests)
                        {
                            var pullRequests = await _gitHub.GetPullRequestsAsync(
                                githubCfg.Org!,
                                repo.RepoName,
                                fromUtc,
                                toUtc,
                                githubCfg.TokenEncrypted!,
                                ct);

                            if (pullRequests.Count > 0)
                            {
                                var prNumbers = pullRequests
                                    .Select(pr => pr.Number)
                                    .Distinct()
                                    .ToList();

                                var existingPullRequests = await _db.GitHubPullRequests
                                    .Where(x => x.RepoId == repo.RepoId && prNumbers.Contains(x.PrNumber))
                                    .ToListAsync(ct);

                                var existingPrByNumber = existingPullRequests
                                    .GroupBy(x => x.PrNumber)
                                    .ToDictionary(g => g.Key, g => g.First());

                                var newPullRequestEntities = new List<GitHubPullRequest>();

                                foreach (var pr in pullRequests)
                                {
                                    if (existingPrByNumber.TryGetValue(pr.Number, out var existingPr))
                                    {
                                        var changed =
                                            existingPr.Title != pr.Title ||
                                            existingPr.Description != pr.Description ||
                                            existingPr.State != pr.State ||
                                            existingPr.AuthorLogin != pr.AuthorLogin ||
                                            existingPr.CreatedAtGithub != pr.CreatedAt ||
                                            existingPr.UpdatedAtGithub != pr.UpdatedAt ||
                                            existingPr.MergedAtGithub != pr.MergedAt ||
                                            existingPr.ClosedAtGithub != pr.ClosedAt ||
                                            existingPr.PrUrl != pr.Url;

                                        if (changed)
                                        {
                                            existingPr.Title = pr.Title.Length > 500 ? pr.Title[..500] : pr.Title;
                                            existingPr.Description = pr.Description;
                                            existingPr.State = pr.State;
                                            existingPr.AuthorLogin = pr.AuthorLogin;
                                            existingPr.CreatedAtGithub = pr.CreatedAt;
                                            existingPr.UpdatedAtGithub = pr.UpdatedAt;
                                            existingPr.MergedAtGithub = pr.MergedAt;
                                            existingPr.ClosedAtGithub = pr.ClosedAt;
                                            existingPr.PrUrl = pr.Url;

                                            prUpdated++;
                                        }
                                        else
                                        {
                                            prSkipped++;
                                        }

                                        pullRequestIdsForSnapshot.Add(existingPr.PullRequestId);
                                        continue;
                                    }

                                    var newPrEntity = new GitHubPullRequest
                                    {
                                        RepoId = repo.RepoId,
                                        PrNumber = pr.Number,
                                        Title = pr.Title.Length > 500 ? pr.Title[..500] : pr.Title,
                                        Description = pr.Description,
                                        State = pr.State,
                                        AuthorLogin = pr.AuthorLogin,
                                        CreatedAtGithub = pr.CreatedAt,
                                        UpdatedAtGithub = pr.UpdatedAt,
                                        MergedAtGithub = pr.MergedAt,
                                        ClosedAtGithub = pr.ClosedAt,
                                        PrUrl = pr.Url
                                    };

                                    newPullRequestEntities.Add(newPrEntity);
                                }

                                if (newPullRequestEntities.Count > 0)
                                {
                                    _db.GitHubPullRequests.AddRange(newPullRequestEntities);
                                    await _db.SaveChangesAsync(ct);

                                    prInserted += newPullRequestEntities.Count;

                                    foreach (var entity in newPullRequestEntities)
                                    {
                                        pullRequestIdsForSnapshot.Add(entity.PullRequestId);
                                    }
                                }

                                await _db.SaveChangesAsync(ct);
                            }
                        }

                        repoOk++;
                    }
                    catch (Exception exRepo)
                    {
                        repoFail++;
                        repoErrors.Add($"{repo.RepoName}: {TrimMsg(exRepo.Message, 200)}");
                    }
                }
                int? snapshotId = null;

                if (commitIdsForSnapshot.Count > 0 || pullRequestIdsForSnapshot.Count > 0)
                {
                    var snapshot = new Snapshot
                    {
                        SyncRunId = run.SyncRunId,
                        CapturedAt = DateTime.UtcNow,
                        Label = $"GitHub sync {fromUtc:yyyy-MM-dd}..{toUtc:yyyy-MM-dd}"
                    };

                    _db.Snapshots.Add(snapshot);
                    await _db.SaveChangesAsync(ct);

                    snapshotId = snapshot.SnapshotId;

                    // SnapshotCommits
                    var existingSnapshotCommitIds = await _db.SnapshotCommits
                        .Where(x => x.SnapshotId == snapshotId.Value)
                        .Select(x => x.CommitId)
                        .ToListAsync(ct);

                    var existingSnapshotCommitIdSet = existingSnapshotCommitIds.ToHashSet();

                    var newSnapshotCommits = commitIdsForSnapshot
                        .Where(commitId => !existingSnapshotCommitIdSet.Contains(commitId))
                        .Select(commitId => new SnapshotCommit
                        {
                            SnapshotId = snapshotId.Value,
                            CommitId = commitId
                        })
                        .ToList();

                    if (newSnapshotCommits.Count > 0)
                    {
                        _db.SnapshotCommits.AddRange(newSnapshotCommits);
                    }

                    // SnapshotPullRequests
                    var existingSnapshotPullRequestIds = await _db.SnapshotPullRequests
                        .Where(x => x.SnapshotId == snapshotId.Value)
                        .Select(x => x.PullRequestId)
                        .ToListAsync(ct);

                    var existingSnapshotPullRequestIdSet = existingSnapshotPullRequestIds.ToHashSet();

                    var newSnapshotPullRequests = pullRequestIdsForSnapshot
                        .Where(pullRequestId => !existingSnapshotPullRequestIdSet.Contains(pullRequestId))
                        .Select(pullRequestId => new SnapshotPullRequest
                        {
                            SnapshotId = snapshotId.Value,
                            PullRequestId = pullRequestId
                        })
                        .ToList();

                    if (newSnapshotPullRequests.Count > 0)
                    {
                        _db.SnapshotPullRequests.AddRange(newSnapshotPullRequests);
                    }

                    await _db.SaveChangesAsync(ct);
                }

                var ok = repoFail == 0;
                var note = ok
                    ? $"OK - Repos OK {repoOk}/{repos.Count}, Commit Inserted {inserted}, Commit Skipped {skipped}, PR Inserted {prInserted}, PR Updated {prUpdated}, PR Skipped {prSkipped}, SnapshotId={(snapshotId.HasValue ? snapshotId.Value : 0)}, Range {fromUtc:yyyy-MM-dd HH:mm:ss} -> {toUtc:yyyy-MM-dd HH:mm:ss} UTC."
                    : $"PARTIAL - Repos OK {repoOk}/{repos.Count}, Failed {repoFail}, Commit Inserted {inserted}, Commit Skipped {skipped}, PR Inserted {prInserted}, PR Updated {prUpdated}, PR Skipped {prSkipped}, SnapshotId={(snapshotId.HasValue ? snapshotId.Value : 0)}, Range {fromUtc:yyyy-MM-dd HH:mm:ss} -> {toUtc:yyyy-MM-dd HH:mm:ss} UTC. Errors: {string.Join(" | ", repoErrors)}";

                return new GitHubSyncResult(ok, TrimMsg(note, 1000), snapshotId);
            }
            catch (Exception ex)
            {
                return new GitHubSyncResult(false, $"FAILED - {TrimMsg(ex.Message)}", null);
            }
        }

        public async Task Handle(RunSyncJobCommand request, CancellationToken ct)
        {
            var run = await _db.SyncRuns.FirstOrDefaultAsync(x => x.SyncRunId == request.SyncRunId, ct);

            if (run is null)
                return;

            if (!string.Equals(run.RunStatus, "RUNNING", StringComparison.OrdinalIgnoreCase))
                return;

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

            int? snapshotId = null;

            // ---------- JIRA ----------
            if (jiraSelected)
            {
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
                        if (FORCE_JIRA_FAIL) throw new Exception("Fake Jira connection failed.");

                        var projectKey = jiraCfg.ProjectKey!.Trim();

                        var jql = run.ScopeType switch
                        {
                            "BACKLOG" => $"project = {projectKey} AND sprint IS EMPTY ORDER BY updated DESC",
                            "SPRINT" => $"project = {projectKey} AND sprint IS NOT EMPTY ORDER BY updated DESC",
                            _ => $"project = {projectKey} ORDER BY updated DESC"
                        };

                        var issues = await _jira.SearchIssuesAsync(
                            jiraCfg.BaseUrl!,
                            jql,
                            maxResults: 100,
                            jiraCfg.TokenEncrypted!,
                            ct
                        );

                        int inserted = 0;
                        int updated = 0;

                        foreach (var it in issues)
                        {
                            var key = (it.IssueKey ?? "").Trim().ToUpperInvariant();
                            if (string.IsNullOrWhiteSpace(key)) continue;

                            var existing = await _db.JiraIssues
                                .FirstOrDefaultAsync(x => x.ProjectId == run.ProjectId && x.IssueKey == key, ct);

                            var now = DateTime.UtcNow;

                            var summary = (it.Summary ?? "").Trim();
                            if (summary.Length > 500) summary = summary[..500];

                            var issueType = MapIssueType(it.IssueType);
                            var priority = MapPriority(it.Priority);
                            var status = MapStatus(it.Status);

                            if (existing is null)
                            {
                                _db.JiraIssues.Add(new JiraIssue
                                {
                                    ProjectId = run.ProjectId,
                                    SprintId = null,          // chưa map sprint
                                    AssigneeUserId = null,    // chưa map user
                                    IssueKey = key,
                                    Summary = summary,
                                    Description = it.Description, // raw JSON ok
                                    IssueType = issueType,
                                    Priority = priority,
                                    Status = status,
                                    StoryPoints = it.StoryPoints,
                                    JiraUrl = it.Url,
                                    CreatedAt = now,
                                    UpdatedAt = now
                                });
                                inserted++;
                            }
                            else
                            {
                                existing.Summary = summary;
                                existing.Description = it.Description;
                                existing.IssueType = issueType;
                                existing.Priority = priority;
                                existing.Status = status;
                                existing.StoryPoints = it.StoryPoints;
                                existing.JiraUrl = it.Url;
                                existing.UpdatedAt = now;
                                updated++;
                            }
                        }

                        await _db.SaveChangesAsync(ct);

                        jiraOk = true;
                        jiraNote = $"SUCCESS - Inserted {inserted}, Updated {updated}.";
                    }
                    catch (Exception ex)
                    {
                        jiraOk = false;
                        jiraNote = $"FAILED - {TrimMsg(ex.Message)}";
                    }
                }
            }

            static string MapStatus(string s)
            {
                s = (s ?? "").Trim().ToUpperInvariant();
                return s switch
                {
                    "TO DO" or "TODO" => "TODO",
                    "IN PROGRESS" => "IN_PROGRESS",
                    "IN REVIEW" or "REVIEW" => "IN_REVIEW",
                    "DONE" => "DONE",
                    "BLOCKED" => "BLOCKED",
                    _ => "TODO"
                };
            }

            static string MapPriority(string s)
            {
                s = (s ?? "").Trim().ToUpperInvariant();
                return s switch
                {
                    "LOW" => "LOW",
                    "MEDIUM" => "MEDIUM",
                    "HIGH" => "HIGH",
                    "HIGHEST" => "HIGHEST",
                    _ => "MEDIUM"
                };
            }

            static string MapIssueType(string s)
            {
                s = (s ?? "").Trim().ToUpperInvariant();
                return s switch
                {
                    "EPIC" => "EPIC",
                    "STORY" => "STORY",
                    "TASK" => "TASK",
                    "BUG" => "BUG",
                    "SUB-TASK" or "SUBTASK" => "SUBTASK",
                    _ => "TASK"
                };
            }

            // ---------- GITHUB ----------
            if (githubSelected)
            {
                var githubResult = await SyncGithubAsync(run, githubCfg, ct);
                githubOk = githubResult.Ok;
                githubNote = githubResult.Note;
                snapshotId = githubResult.SnapshotId;
            }

            string linksNote = "N/A";

            //if (jiraSelected && githubSelected && jiraOk && githubOk && snapshotId.HasValue)
            //{
            //    try
            //    {
            //        var (ok, note) = await BuildIssueCommitLinksAsync(snapshotId.Value, run.ProjectId, ct);
            //        linksNote = ok ? note : $"FAILED - {note}";
            //    }
            //    catch (Exception ex)
            //    {
            //        linksNote = $"FAILED - {TrimMsg(ex.Message)}";
            //    }
            //}
            //else if (jiraSelected && githubSelected)
            //{
            //    linksNote = "SKIPPED - Need both Jira and GitHub SUCCESS and a Snapshot.";
            //}

            if (jiraSelected && githubSelected && jiraOk && githubOk && snapshotId.HasValue && run.SyncGithubCommits)
            {
                try
                {
                    var hasSnapshotCommits = await _db.SnapshotCommits
                        .AnyAsync(x => x.SnapshotId == snapshotId.Value, ct);

                    if (hasSnapshotCommits)
                    {
                        var (ok, note) = await BuildIssueCommitLinksAsync(snapshotId.Value, run.ProjectId, ct);
                        linksNote = ok ? note : $"FAILED - {note}";
                    }
                    else
                    {
                        linksNote = "SKIPPED - No commits in snapshot for linking.";
                    }
                }
                catch (Exception ex)
                {
                    linksNote = $"FAILED - {TrimMsg(ex.Message)}";
                }
            }
            else if (jiraSelected && githubSelected)
            {
                linksNote = "SKIPPED - Need both Jira and GitHub SUCCESS, a Snapshot, and GitHub commit sync.";
            }

            //run.Notes = $"JIRA: {jiraNote}; GITHUB: {githubNote}";
            run.Notes = $"JIRA: {jiraNote}; GITHUB: {githubNote}; LINKS: {linksNote}";
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

        //private static string TrimMsg(string msg)
        //    => (msg ?? "").Length <= 200 ? (msg ?? "") : (msg ?? "")[..200];

        private static string TrimMsg(string? message, int maxLength = 500)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            return message.Length <= maxLength
                ? message
                : message[..maxLength];
        }

        private static readonly System.Text.RegularExpressions.Regex IssueKeyRx =
            new(@"\b[A-Z][A-Z0-9]+-\d+\b", System.Text.RegularExpressions.RegexOptions.Compiled);

        private async Task<(bool ok, string note)> BuildIssueCommitLinksAsync(int snapshotId, int projectId, CancellationToken ct)
        {
            // Map issueKey -> issueId
            var issueMap = await _db.JiraIssues
                .AsNoTracking()
                .Where(i => i.ProjectId == projectId)
                .Select(i => new { i.IssueId, i.IssueKey })
                .ToDictionaryAsync(x => x.IssueKey.ToUpper(), x => x.IssueId, ct);

            if (issueMap.Count == 0)
                return (false, "No JiraIssues for this project.");

            // Commits in snapshot
            var commits = await (
                from sc in _db.SnapshotCommits.AsNoTracking()
                join c in _db.GitHubCommits.AsNoTracking() on sc.CommitId equals c.CommitId
                where sc.SnapshotId == snapshotId
                select new { c.CommitId, c.Message }
            ).ToListAsync(ct);

            if (commits.Count == 0)
                return (false, "No commits in snapshot.");

            // Existing links (avoid duplicate insert)
            var existing = await _db.IssueCommitLinks
                .AsNoTracking()
                .Where(x => x.SnapshotId == snapshotId)
                .Select(x => new { x.IssueId, x.CommitId })
                .ToListAsync(ct);

            var existsSet = existing.Select(x => (x.IssueId, x.CommitId)).ToHashSet();

            int inserted = 0;

            foreach (var c in commits)
            {
                var keys = IssueKeyRx.Matches(c.Message ?? "")
                    .Select(m => m.Value.ToUpperInvariant())
                    .Distinct();

                foreach (var key in keys)
                {
                    if (!issueMap.TryGetValue(key, out var issueId)) continue;

                    if (existsSet.Contains((issueId, c.CommitId))) continue;

                    _db.IssueCommitLinks.Add(new IssueCommitLink
                    {
                        SnapshotId = snapshotId,
                        IssueId = issueId,
                        CommitId = c.CommitId
                    });

                    existsSet.Add((issueId, c.CommitId));
                    inserted++;
                }
            }

            if (inserted == 0)
                return (true, "SUCCESS - 0 new links.");

            await _db.SaveChangesAsync(ct);
            return (true, $"SUCCESS - Inserted {inserted} links.");
        }
    }
}
