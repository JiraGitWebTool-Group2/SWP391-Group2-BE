using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Sync.Command
{
    public class RunSyncJobHandler : IRequestHandler<RunSyncJobCommand>
    {
        private readonly IApplicationDbContext _db;

        public RunSyncJobHandler(IApplicationDbContext db)
        {
            _db = db;
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
                        await Task.Delay(800, ct); // fake call

                        if (FORCE_GITHUB_FAIL) throw new Exception("Fake GitHub connection failed.");

                        // TODO: sau này: gọi GitHub API thật bằng githubCfg + repositories
                        githubOk = true;
                        githubNote = "SUCCESS (fake)";
                    }
                    catch (Exception ex)
                    {
                        githubOk = false;
                        githubNote = $"FAILED - {TrimMsg(ex.Message)}";
                    }
                }
            }

            // Snapshot luôn tạo (kể cả rỗng)
            _db.Snapshots.Add(new Snapshot
            {
                SyncRunId = run.SyncRunId,
                CapturedAt = DateTime.UtcNow,
                Label = "Sync snapshot"
            });

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
