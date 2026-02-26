using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Snapshots.Dtos;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public class GetGroupSnapshotsHandler : IRequestHandler<GetGroupSnapshotsQuery, List<SnapshotListItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetGroupSnapshotsHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SnapshotListItemDto>> Handle(GetGroupSnapshotsQuery request, CancellationToken cancellationToken)
        {
            var groupId = request.GroupId;

            var exists = await _db.Groups
                .AsNoTracking()
                .AnyAsync(g => g.GroupId == groupId, cancellationToken);

            if (!exists)
                throw new KeyNotFoundException($"Group {groupId} not found.");

            var data = await _db.Snapshots
                .AsNoTracking()
                .Where(s => s.SyncRun.Project.GroupId == groupId)
                .OrderByDescending(s => s.CapturedAt)
                .Select(s => new SnapshotListItemDto(
                    s.SnapshotId,
                    s.CapturedAt,
                    s.Label,
                    new SyncRunBriefDto(
                        s.SyncRun.SyncRunId,
                        s.SyncRun.ProjectId,
                        s.SyncRun.Project.ProjectName,
                        s.SyncRun.TriggerType,
                        s.SyncRun.ScopeType,
                        s.SyncRun.SprintId,
                        s.SyncRun.RunStatus,
                        s.SyncRun.StartedAt,
                        s.SyncRun.FinishedAt
                    )
                ))
                .Take(200)
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}
