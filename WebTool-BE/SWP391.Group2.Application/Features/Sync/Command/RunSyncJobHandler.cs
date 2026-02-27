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

            // Nếu job bị gọi lại khi đã xong thì bỏ qua (idempotent nhẹ)
            if (run.RunStatus != "RUNNING") return;

            bool jiraSelected = run.IncludeJira;
            bool githubSelected = run.IncludeGithub;

            // Nếu vì lý do nào đó DB có include_jira/include_github = false hết
            // thì fail (nhưng normal flow đã validate ở StartSync rồi)
            if (!jiraSelected && !githubSelected)
            {
                run.RunStatus = "FAILED";
                run.FinishedAt = DateTime.UtcNow;
                run.Notes = "No source selected.";
                await _db.SaveChangesAsync(ct);
                return;
            }

            // ====== FAKE FLAGS để bạn test lỗi/partial ======
            // Đổi true/false tùy lúc bạn muốn test
            const bool FORCE_JIRA_FAIL = true;
            const bool FORCE_GITHUB_FAIL = false;
            // ==============================================

            bool jiraOk = !jiraSelected;   // nếu không chọn -> coi như "N/A" (không làm, không fail)
            bool githubOk = !githubSelected;

            string jiraNote = jiraSelected ? "SKIPPED" : "N/A";
            string githubNote = githubSelected ? "SKIPPED" : "N/A";

            try
            {
                // giả lập chạy
                if (jiraSelected)
                {
                    await Task.Delay(800, ct);

                    if (FORCE_JIRA_FAIL) throw new Exception("Fake Jira connection failed.");

                    // TODO: sau này gọi Jira API + upsert
                    jiraOk = true;
                    jiraNote = "SUCCESS (fake)";
                }
            }
            catch (Exception ex)
            {
                jiraOk = false;
                jiraNote = $"FAILED - {TrimMsg(ex.Message)}";
            }

            try
            {
                if (githubSelected)
                {
                    await Task.Delay(800, ct);

                    if (FORCE_GITHUB_FAIL) throw new Exception("Fake GitHub connection failed.");

                    // TODO: sau này gọi GitHub API + upsert
                    githubOk = true;
                    githubNote = "SUCCESS (fake)";
                }
            }
            catch (Exception ex)
            {
                githubOk = false;
                githubNote = $"FAILED - {TrimMsg(ex.Message)}";
            }

            // tạo snapshot dù data rỗng cũng ok (MF-1 AF-5)
            _db.Snapshots.Add(new Snapshot
            {
                SyncRunId = run.SyncRunId,
                CapturedAt = DateTime.UtcNow,
                Label = "Fake sync"
            });

            run.Notes = $"JIRA: {jiraNote}; GITHUB: {githubNote}";
            run.FinishedAt = DateTime.UtcNow;

            // quyết định status cuối
            // - nếu chọn 1 nguồn: status = SUCCESS/FAILED theo nguồn đó
            // - nếu chọn 2 nguồn: SUCCESS nếu cả 2 OK, FAILED nếu cả 2 fail, PARTIAL nếu 1 OK 1 fail
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
            else // githubSelected
            {
                run.RunStatus = githubOk ? "SUCCESS" : "FAILED";
            }

            await _db.SaveChangesAsync(ct);
        }

        private static string TrimMsg(string msg)
            => (msg ?? "").Length <= 200 ? (msg ?? "") : (msg ?? "")[..200];
    }
}
