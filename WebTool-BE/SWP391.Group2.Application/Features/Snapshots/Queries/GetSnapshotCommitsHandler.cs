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
    public class GetSnapshotCommitsHandler : IRequestHandler<GetSnapshotCommitsQuery, IReadOnlyList<SnapshotCommitItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetSnapshotCommitsHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<SnapshotCommitItemDto>> Handle(GetSnapshotCommitsQuery request, CancellationToken ct)
        {
            // verify snapshot exists (optional nhưng giúp trả 404 rõ ràng)
            var exists = await _db.Snapshots.AnyAsync(s => s.SnapshotId == request.SnapshotId, ct);
            if (!exists) throw new KeyNotFoundException("Snapshot not found.");

            // SnapshotCommits -> GitHubCommits -> Repositories
            var items = await (
                from sc in _db.SnapshotCommits.AsNoTracking()
                join c in _db.GitHubCommits.AsNoTracking() on sc.CommitId equals c.CommitId
                join r in _db.Repositories.AsNoTracking() on c.RepoId equals r.RepoId
                where sc.SnapshotId == request.SnapshotId
                orderby c.CommittedAt descending
                select new SnapshotCommitItemDto(
                    c.CommitId,
                    c.CommitHash,
                    c.Message,
                    c.CommittedAt,
                    c.CommitUrl,
                    r.RepoId,
                    r.RepoName
                )
            ).ToListAsync(ct);

            return items;
        }
    }
}
