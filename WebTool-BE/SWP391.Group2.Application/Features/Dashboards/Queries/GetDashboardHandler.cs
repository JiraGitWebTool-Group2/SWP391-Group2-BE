using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Dashboards.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Dashboards.Queries
{
    public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
    {
        private readonly IApplicationDbContext _db;

        public GetDashboardHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            var groupId = request.GroupId;

            var exists = await _db.Groups
                .AsNoTracking()
                .AnyAsync(g => g.GroupId == groupId, cancellationToken);

            if (!exists)
                throw new KeyNotFoundException($"Group {groupId} not found.");

            var latestSnapshot = await _db.Snapshots
                .AsNoTracking()
                .Where(s => s.SyncRun.Project.GroupId == groupId)
                .OrderByDescending(s => s.CapturedAt)
                .Select(s => new { s.SnapshotId, s.CapturedAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (latestSnapshot is null)
                throw new KeyNotFoundException("Group chưa có snapshot nào để thống kê.");

            var snapshotId = latestSnapshot.SnapshotId;

            // Issue stats theo group
            var issuesQuery = _db.JiraIssues
                .AsNoTracking()
                .Where(i => i.Project.GroupId == groupId);

            var totalIssues = await issuesQuery.CountAsync(cancellationToken);

            var byStatus = await issuesQuery
                .GroupBy(x => x.Status)
                .Select(g => new { k = g.Key, v = g.Count() })
                .ToDictionaryAsync(x => x.k, x => x.v, cancellationToken);

            var byType = await issuesQuery
                .GroupBy(x => x.IssueType)
                .Select(g => new { k = g.Key, v = g.Count() })
                .ToDictionaryAsync(x => x.k, x => x.v, cancellationToken);

            var byPriority = await issuesQuery
                .GroupBy(x => x.Priority)
                .Select(g => new { k = g.Key, v = g.Count() })
                .ToDictionaryAsync(x => x.k, x => x.v, cancellationToken);

            // Commit/link stats theo snapshot mới nhất
            var linksQuery = _db.IssueCommitLinks
                .AsNoTracking()
                .Where(l => l.SnapshotId == snapshotId);

            var linksCount = await linksQuery.CountAsync(cancellationToken);
            var linkedIssuesCount = await linksQuery.Select(l => l.IssueId).Distinct().CountAsync(cancellationToken);

            var commitIds = await linksQuery.Select(l => l.CommitId).Distinct().ToListAsync(cancellationToken);
            var totalCommitsInSnapshot = commitIds.Count;

            var top = await _db.GitHubCommits
                .AsNoTracking()
                .Where(c => commitIds.Contains(c.CommitId))
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, Commits = g.Count() })
                .OrderByDescending(x => x.Commits)
                .Take(5)
                .ToListAsync(cancellationToken);

            var userIds = top.Where(x => x.UserId != null).Select(x => x.UserId!.Value).ToList();

            var userMap = await _db.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.FullName, cancellationToken);

            var topContributors = top.Select(x =>
            {
                var name = (x.UserId != null && userMap.TryGetValue(x.UserId.Value, out var fullName))
                    ? fullName
                    : "Unknown";
                return new ContributorDto(x.UserId, name, x.Commits);
            }).ToList();

            return new DashboardDto(
                groupId,
                snapshotId,
                latestSnapshot.CapturedAt,
                new IssueStatsDto(totalIssues, byStatus, byType, byPriority),
                new CommitStatsDto(totalCommitsInSnapshot, linkedIssuesCount, linksCount, topContributors)
            );
        }
    }
}
