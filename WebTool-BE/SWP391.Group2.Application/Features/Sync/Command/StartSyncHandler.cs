using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Abstractions.Jobs;
using SWP391.Group2.Domain.Entities;

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

            // ===== GitHub options normalize / validate =====
            DateTime? githubFrom = null;
            DateTime? githubTo = null;
            bool syncGithubCommits = request.SyncGithubCommits;
            bool syncGithubPullRequests = request.SyncGithubPullRequests;
            string? githubSyncMode = string.IsNullOrWhiteSpace(request.GithubSyncMode)
                ? null
                : request.GithubSyncMode.Trim().ToUpperInvariant();

            if (request.IncludeGithub)
            {
                if (!syncGithubCommits && !syncGithubPullRequests)
                    throw new ArgumentException("At least one GitHub item must be selected.");

                if (request.GithubFrom.HasValue ^ request.GithubTo.HasValue)
                    throw new ArgumentException("GithubFrom and GithubTo must be provided together.");

                if (request.GithubFrom.HasValue && request.GithubTo.HasValue)
                {
                    githubFrom = DateTime.SpecifyKind(request.GithubFrom.Value, DateTimeKind.Utc);
                    githubTo = DateTime.SpecifyKind(request.GithubTo.Value, DateTimeKind.Utc);

                    if (githubFrom > githubTo)
                        throw new ArgumentException("GithubFrom must be less than or equal to GithubTo.");

                    githubSyncMode ??= githubFrom.Value.Date == githubTo.Value.Date
                        ? "SINGLE_DAY"
                        : "CUSTOM_RANGE";
                }
                else
                {
                    githubTo = DateTime.UtcNow;
                    githubFrom = githubTo.Value.AddDays(-7);

                    githubSyncMode ??= "INCREMENTAL";
                }

                if (githubSyncMode is not ("FULL" or "INCREMENTAL" or "CUSTOM_RANGE" or "SINGLE_DAY"))
                    throw new ArgumentException("GithubSyncMode is invalid.");
            }
            else
            {
                // Không sync GitHub thì dọn sạch metadata GitHub
                syncGithubCommits = false;
                syncGithubPullRequests = false;
                githubSyncMode = null;
                githubFrom = null;
                githubTo = null;
            }

            var run = new SyncRun
            {
                ProjectId = request.ProjectId,
                TriggeredByUserId = request.TriggeredByUserId,
                TriggerType = triggerType,
                ScopeType = scope,
                SprintId = request.SprintId,
                IncludeJira = request.IncludeJira,
                IncludeGithub = request.IncludeGithub,

                GithubFrom = githubFrom,
                GithubTo = githubTo,
                SyncGithubCommits = syncGithubCommits,
                SyncGithubPullRequests = syncGithubPullRequests,
                GithubSyncMode = githubSyncMode,

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