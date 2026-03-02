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
    public class GetSnapshotTraceabilityHandler
    : IRequestHandler<GetSnapshotTraceabilityQuery, List<TraceabilityItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetSnapshotTraceabilityHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<TraceabilityItemDto>> Handle(GetSnapshotTraceabilityQuery request, CancellationToken ct)
        {
            var snapshotExists = await _db.Snapshots
                .AsNoTracking()
                .AnyAsync(s => s.SnapshotId == request.SnapshotId, ct);

            if (!snapshotExists)
                throw new KeyNotFoundException("Snapshot not found.");

            // IssueCommitLinks(snapshot_id, issue_id, commit_id)
            // Join JiraIssues + GitHubCommits, group by issue
            var query =
                from l in _db.IssueCommitLinks.AsNoTracking()
                join i in _db.JiraIssues.AsNoTracking() on l.IssueId equals i.IssueId
                join c in _db.GitHubCommits.AsNoTracking() on l.CommitId equals c.CommitId
                where l.SnapshotId == request.SnapshotId
                group new { i, c } by new
                {
                    i.IssueId,
                    i.IssueKey,
                    i.Summary,
                    i.Status,
                    i.Priority,
                    i.IssueType,
                    i.StoryPoints,
                    i.JiraUrl
                }
                into g
                orderby g.Key.IssueKey
                select new TraceabilityItemDto
                {
                    IssueId = g.Key.IssueId,
                    IssueKey = g.Key.IssueKey,
                    Summary = g.Key.Summary,
                    Status = g.Key.Status,
                    Priority = g.Key.Priority,
                    IssueType = g.Key.IssueType,
                    StoryPoints = g.Key.StoryPoints,
                    JiraUrl = g.Key.JiraUrl,

                    CommitCount = g.Count(),
                    LatestCommitAt = g.Max(x => (DateTime?)x.c.CommittedAt)
                };

            return await query.ToListAsync(ct);
        }
    }
}
