using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Sync.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Sync.Queries
{
    public class GetSyncRunStatusHandler : IRequestHandler<GetSyncRunStatusQuery, SyncRunStatusDto?>
    {
        private readonly IApplicationDbContext _db;

        public GetSyncRunStatusHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SyncRunStatusDto?> Handle(GetSyncRunStatusQuery request, CancellationToken ct)
        {
            var run = await _db.SyncRuns
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SyncRunId == request.SyncRunId, ct);

            if (run is null)
                return null;

            var snapshotId = await _db.Snapshots
                .AsNoTracking()
                .Where(s => s.SyncRunId == run.SyncRunId)
                .OrderByDescending(s => s.CapturedAt)
                .Select(s => (int?)s.SnapshotId)
                .FirstOrDefaultAsync(ct);

            return new SyncRunStatusDto(
                run.SyncRunId,
                run.ProjectId,
                run.RunStatus,
                run.Notes,
                run.StartedAt,
                run.FinishedAt,
                run.TriggeredByUserId,
                //run.TriggeredByRole,
                snapshotId
            );
        }
    }
}
