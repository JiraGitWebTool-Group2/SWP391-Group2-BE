using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Features.Groups.Dtos;
using SWP391.Group2.Infrastructure.Persistence;

namespace SWP391.Group2.Api.Controllers
{
    public class DashboardController : Controller
    {
        // =========================
        // #11: GET /api/groups/{groupId}/dashboard
        // Lấy thống kê issue & commit (commit/link theo snapshot mới nhất)
        // =========================
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet("{groupId:int}/dashboard")]

        public async Task<ActionResult<DashboardDto>> GetDashboard(int groupId)
        {
            var exists = await _db.Groups.AsNoTracking().AnyAsync(g => g.GroupId == groupId);
            if (!exists) return NotFound(new { message = $"Group {groupId} not found." });

            var latestSnapshot = await _db.Snapshots
                .AsNoTracking()
                .Where(s => s.SyncRun.Project.GroupId == groupId)
                .OrderByDescending(s => s.CapturedAt)
                .Select(s => new { s.SnapshotId, s.CapturedAt })
                .FirstOrDefaultAsync();

            if (latestSnapshot is null)
                return NotFound(new { message = "Group chưa có snapshot nào để thống kê." });

            var snapshotId = latestSnapshot.SnapshotId;

            // Issue stats theo group (không snapshot-based trong schema)
            var issuesQuery = _db.JiraIssues
                .AsNoTracking()
                .Where(i => i.Project.GroupId == groupId);

            var totalIssues = await issuesQuery.CountAsync();
            var byStatus = await issuesQuery
                .GroupBy(x => x.Status)
                .Select(g => new { k = g.Key, v = g.Count() })
                .ToDictionaryAsync(x => x.k, x => x.v);

            var byType = await issuesQuery
                .GroupBy(x => x.IssueType)
                .Select(g => new { k = g.Key, v = g.Count() })
                .ToDictionaryAsync(x => x.k, x => x.v);

            var byPriority = await issuesQuery
                .GroupBy(x => x.Priority)
                .Select(g => new { k = g.Key, v = g.Count() })
                .ToDictionaryAsync(x => x.k, x => x.v);

            // Commit/link stats theo snapshot mới nhất
            var linksQuery = _db.IssueCommitLinks
                .AsNoTracking()
                .Where(l => l.SnapshotId == snapshotId);

            var linksCount = await linksQuery.CountAsync();
            var linkedIssuesCount = await linksQuery.Select(l => l.IssueId).Distinct().CountAsync();
            var commitIds = await linksQuery.Select(l => l.CommitId).Distinct().ToListAsync();

            var totalCommitsInSnapshot = commitIds.Count;

            var top = await _db.GitHubCommits
                .AsNoTracking()
                .Where(c => commitIds.Contains(c.CommitId))
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, Commits = g.Count() })
                .OrderByDescending(x => x.Commits)
                .Take(5)
                .ToListAsync();

            var userIds = top.Where(x => x.UserId != null).Select(x => x.UserId!.Value).ToList();
            var userMap = await _db.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.FullName);

            var topContributors = top.Select(x =>
            {
                var name = (x.UserId != null && userMap.TryGetValue(x.UserId.Value, out var fullName))
                    ? fullName
                    : "Unknown";
                return new ContributorDto(x.UserId, name, x.Commits);
            }).ToList();

            var dto = new DashboardDto(
                groupId,
                snapshotId,
                latestSnapshot.CapturedAt,
                new IssueStatsDto(totalIssues, byStatus, byType, byPriority),
                new CommitStatsDto(totalCommitsInSnapshot, linkedIssuesCount, linksCount, topContributors)
            );

            return Ok(dto);
        }
    }
}
