using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Abstractions.Jobs;
using SWP391.Group2.Domain.Entities;
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
            if (!request.IncludeJira && !request.IncludeGithub)
                throw new ArgumentException("Must select at least one source (Jira or GitHub).");

            var scope = (request.ScopeType ?? string.Empty).Trim().ToUpperInvariant();
            if (scope is not ("SPRINT" or "BACKLOG" or "CUSTOM"))
                throw new ArgumentException("Invalid scope type.");

            if (scope == "SPRINT" && request.SprintId is null)
                throw new ArgumentException("SprintId is required when scope type is SPRINT.");

            var triggerType = (request.TriggerType ?? string.Empty).Trim().ToUpperInvariant();
            if (triggerType is not ("MANUAL" or "AUTO"))
                throw new ArgumentException("TriggerType must be MANUAL or AUTO.");

            string? triggeredByRole = null;

            if (triggerType == "AUTO")
            {
                if (request.TriggeredByUserId.HasValue)
                    throw new ArgumentException("TriggeredByUserId must be null when TriggerType is AUTO.");
            }
            else
            {
                if (!request.TriggeredByUserId.HasValue)
                    throw new ArgumentException("TriggeredByUserId is required when TriggerType is MANUAL.");

                triggeredByRole = await ResolveProjectRoleAsync(
                    request.ProjectId,
                    request.TriggeredByUserId.Value,
                    ct);

                if (string.IsNullOrWhiteSpace(triggeredByRole))
                    throw new ArgumentException("User does not belong to this project's group or has no project role.");
            }

            var run = new SyncRun
            {
                ProjectId = request.ProjectId,
                TriggeredByUserId = request.TriggeredByUserId,
                //TriggeredByRole = triggeredByRole,
                TriggerType = triggerType,
                ScopeType = scope,
                SprintId = request.SprintId,
                IncludeJira = request.IncludeJira,
                IncludeGithub = request.IncludeGithub,
                RunStatus = "RUNNING",
                StartedAt = DateTime.UtcNow
            };

            _db.SyncRuns.Add(run);
            await _db.SaveChangesAsync(ct);

            _queue.EnqueueSyncRun(run.SyncRunId);

            return run.SyncRunId;
        }

        private async Task<string?> ResolveProjectRoleAsync(int projectId, int userId, CancellationToken ct)
        {
            var roleName = await (
                from p in _db.Projects
                join ug in _db.UserGroups on p.GroupId equals ug.GroupId
                join r in _db.Roles on ug.RoleId equals r.RoleId
                where p.ProjectId == projectId
                      && ug.UserId == userId
                      && ug.IsActive
                select r.RoleName
            ).FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(roleName))
                return null;

            return roleName.Trim().ToUpperInvariant();
        }
    }
}
