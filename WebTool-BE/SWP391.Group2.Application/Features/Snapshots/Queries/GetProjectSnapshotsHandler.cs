using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Snapshots.Dtos;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public class GetProjectSnapshotsHandler
        : IRequestHandler<GetProjectSnapshotsQuery, List<SnapshotListItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetProjectSnapshotsHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SnapshotListItemDto>> Handle(
            GetProjectSnapshotsQuery request,
            CancellationToken cancellationToken)
        {
            var projectExists = await _db.Projects
                .AsNoTracking()
                .AnyAsync(p => p.ProjectId == request.ProjectId, cancellationToken);

            if (!projectExists)
                throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

            var data = await _db.Snapshots
                .AsNoTracking()
                .Where(s => s.SyncRun.ProjectId == request.ProjectId)
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