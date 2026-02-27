using MediatR;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Abstractions.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Sync.Command
{
    public class StartSyncHandler : IRequestHandler<StartSyncCommand, int>
    {
        private readonly IApplicationDbContext _db;
        private readonly IBackgroundJobQueue _queue;

        public StartSyncHandler(IApplicationDbContext db, IBackgroundJobQueue queue)
        {
            _db = db;
            _queue = queue;
        }

        public async Task<int> Handle(StartSyncCommand request, CancellationToken ct)
        {
            // Validate: phải chọn ít nhất 1 nguồn
            if (!request.IncludeJira && !request.IncludeGithub)
                throw new ArgumentException("Must select at least one source (Jira or GitHub).");

            // Validate: scope_type hợp lệ
            var scope = request.ScopeType?.Trim().ToUpperInvariant();
            if (scope is not ("SPRINT" or "BACKLOG" or "CUSTOM"))
                throw new ArgumentException("Invalid scope type.");

            if (scope == "SPRINT" && request.SprintId is null)
                throw new ArgumentException("SprintId is required when scope type is SPRINT.");

            // Tạo SyncRun RUNNING
            //var run = new SyncRun
            var run = new SWP391.Group2.Domain.Entities.SyncRun
            {
                ProjectId = request.ProjectId,
                TriggeredByUserId = request.TriggeredByUserId,
                TriggerType = request.TriggerType, // MANUAL/AUTO
                ScopeType = scope,                 // SPRINT/BACKLOG/CUSTOM
                SprintId = request.SprintId,
                IncludeJira = request.IncludeJira,
                IncludeGithub = request.IncludeGithub,
                RunStatus = "RUNNING",
                StartedAt = DateTime.UtcNow
            };

            _db.SyncRuns.Add(run);
            await _db.SaveChangesAsync(ct);

            // Đẩy job chạy nền
            _queue.EnqueueSyncRun(run.SyncRunId);

            return run.SyncRunId;
        }
    }
}
