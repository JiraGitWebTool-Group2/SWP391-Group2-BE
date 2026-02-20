using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.SyncRun.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.SyncRun.Queries
{
    public class GetSyncDetailHandler
    : IRequestHandler<GetSyncDetailQuery, SyncDetailDto>
    {
        private readonly IApplicationDbContext _context;

        public GetSyncDetailHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SyncDetailDto> Handle(
            GetSyncDetailQuery request,
            CancellationToken cancellationToken)
        {
            var sync = await _context.SyncRuns
                .Include(x => x.Snapshots)
                .FirstOrDefaultAsync(x => x.SyncRunId == request.SyncId, cancellationToken);

            if (sync == null)
                throw new Exception("Sync not found.");

            return new SyncDetailDto
            {
                SyncRunId = sync.SyncRunId,
                ProjectId = sync.ProjectId,
                TriggerType = sync.TriggerType,
                ScopeType = sync.ScopeType,
                RunStatus = sync.RunStatus,
                IncludeJira = sync.IncludeJira,
                IncludeGithub = sync.IncludeGithub,
                StartedAt = sync.StartedAt,
                FinishedAt = sync.FinishedAt,
                SnapshotCount = sync.Snapshots.Count
            };
        }
    }
}
