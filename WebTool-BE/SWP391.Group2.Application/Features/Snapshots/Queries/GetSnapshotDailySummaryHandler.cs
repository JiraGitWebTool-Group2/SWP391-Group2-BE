using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Snapshots.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public class GetSnapshotDailySummaryHandler
    : IRequestHandler<GetSnapshotDailySummaryQuery, List<SnapshotDailySummaryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetSnapshotDailySummaryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SnapshotDailySummaryDto>> Handle(
            GetSnapshotDailySummaryQuery request,
            CancellationToken cancellationToken)
        {
            // Optional guard: snapshot tồn tại không?
            var snapshotExists = await _context.Snapshots
                .AsNoTracking()
                .AnyAsync(s => s.SnapshotId == request.SnapshotId, cancellationToken);

            if (!snapshotExists)
                return new List<SnapshotDailySummaryDto>();

            var query =
                from sc in _context.SnapshotCommits.AsNoTracking()
                join c in _context.GitHubCommits.AsNoTracking()
                    on sc.CommitId equals c.CommitId
                where sc.SnapshotId == request.SnapshotId
                group c by c.CommittedAt.Date into g
                orderby g.Key
                select new SnapshotDailySummaryDto
                {
                    Date = g.Key,
                    TotalCommits = g.Count(),
                    // user_id có thể null -> Distinct trên nullable là ok
                    DistinctContributors = g.Select(x => x.UserId).Distinct().Count(),
                    DistinctRepositories = g.Select(x => x.RepoId).Distinct().Count()
                };

            return await query.ToListAsync(cancellationToken);
        }
    }
}
