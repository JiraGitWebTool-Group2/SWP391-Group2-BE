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
    public class GetSnapshotRepoSummaryHandler
    : IRequestHandler<GetSnapshotRepoSummaryQuery, IReadOnlyList<SnapshotRepoSummaryDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetSnapshotRepoSummaryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<SnapshotRepoSummaryDto>> Handle(GetSnapshotRepoSummaryQuery request, CancellationToken ct)
        {
            var exists = await _db.Snapshots.AnyAsync(s => s.SnapshotId == request.SnapshotId, ct);
            if (!exists) throw new KeyNotFoundException("Snapshot not found.");

            // SnapshotCommits -> GitHubCommits -> Repositories -> group by repo
            var result = await (
                from sc in _db.SnapshotCommits.AsNoTracking()
                join c in _db.GitHubCommits.AsNoTracking() on sc.CommitId equals c.CommitId
                join r in _db.Repositories.AsNoTracking() on c.RepoId equals r.RepoId
                where sc.SnapshotId == request.SnapshotId
                group r by new { r.RepoId, r.RepoName } into g
                orderby g.Count() descending, g.Key.RepoName
                select new SnapshotRepoSummaryDto(
                    g.Key.RepoId,
                    g.Key.RepoName,
                    g.Count()
                )
            ).ToListAsync(ct);

            return result;
        }
    }
}
