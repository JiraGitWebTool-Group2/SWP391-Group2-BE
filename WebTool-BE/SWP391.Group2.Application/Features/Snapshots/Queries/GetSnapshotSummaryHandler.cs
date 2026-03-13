using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Snapshots.Dtos;

using System;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public class GetSnapshotSummaryHandler
        : IRequestHandler<GetSnapshotSummaryQuery, SnapshotSummaryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetSnapshotSummaryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SnapshotSummaryDto> Handle(
            GetSnapshotSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var snapshotExists = await _context.Snapshots
                .AnyAsync(x => x.SnapshotId == request.SnapshotId, cancellationToken);

            if (!snapshotExists)
                throw new KeyNotFoundException("Snapshot not found");

            var commits = await (
                from sc in _context.SnapshotCommits
                join c in _context.GitHubCommits on sc.CommitId equals c.CommitId
                where sc.SnapshotId == request.SnapshotId
                select c
            ).ToListAsync(cancellationToken);

            return new SnapshotSummaryDto
            {
                SnapshotId = request.SnapshotId,
                TotalCommits = commits.Count,
                DistinctRepositories = commits.Select(x => x.RepoId).Distinct().Count(),
                DistinctContributors = commits
                    .Where(x => x.UserId != null)
                    .Select(x => x.UserId)
                    .Distinct()
                    .Count()
            };
        }
    }
}