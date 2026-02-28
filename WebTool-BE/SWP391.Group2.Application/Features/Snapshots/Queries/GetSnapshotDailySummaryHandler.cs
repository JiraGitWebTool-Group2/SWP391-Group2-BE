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
    public sealed class GetSnapshotDailySummaryHandler
    : IRequestHandler<GetSnapshotDailySummaryQuery, SnapshotDailySummaryDto>
    {
        private readonly IApplicationDbContext _db;

        public GetSnapshotDailySummaryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SnapshotDailySummaryDto> Handle(GetSnapshotDailySummaryQuery request, CancellationToken ct)
        {
            // 1) snapshot tồn tại?
            var snapshot = await _db.Snapshots
                .AsNoTracking()
                .Where(s => s.SnapshotId == request.SnapshotId)
                .Select(s => new { s.SnapshotId, s.CapturedAt })
                .SingleOrDefaultAsync(ct);

            if (snapshot is null)
                throw new KeyNotFoundException($"Snapshot {request.SnapshotId} not found.");

            // 2) lấy commits thuộc snapshot (đúng snapshot isolation)
            var rows = await (
                from sc in _db.SnapshotCommits.AsNoTracking()
                join c in _db.GitHubCommits.AsNoTracking() on sc.CommitId equals c.CommitId
                join r in _db.Repositories.AsNoTracking() on c.RepoId equals r.RepoId
                join u in _db.Users.AsNoTracking() on c.UserId equals u.UserId into users
                from u in users.DefaultIfEmpty()
                where sc.SnapshotId == request.SnapshotId
                select new
                {
                    c.CommittedAt,
                    RepoId = r.RepoId,
                    AuthorName = u != null
                        ? (u.FullName ?? u.Email)
                        : "Unknown"
                }
            ).ToListAsync(ct);

            // 3) group theo ngày (có offset nếu cần)
            var days = rows
                .GroupBy(x => DateOnly.FromDateTime(x.CommittedAt.AddMinutes(request.TzOffsetMinutes)))
                .OrderBy(g => g.Key)
                .Select(g => new SnapshotDailyBucketDto
                {
                    Date = g.Key,
                    CommitCount = g.Count(),
                    ActiveRepoCount = g.Select(x => x.RepoId).Distinct().Count(),
                    TopAuthors = g.GroupBy(x => x.AuthorName)
                        .Select(ag => new AuthorCountDto(ag.Key, ag.Count()))
                        .OrderByDescending(a => a.Count)
                        .ThenBy(a => a.Name)
                        .Take(5)
                        .ToList()
                })
                .ToList();

            return new SnapshotDailySummaryDto
            {
                SnapshotId = snapshot.SnapshotId,
                CapturedAt = snapshot.CapturedAt,
                Days = days
            };
        }
    }
}
