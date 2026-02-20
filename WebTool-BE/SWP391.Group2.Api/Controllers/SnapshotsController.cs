using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Features.Groups.Dtos;
using SWP391.Group2.Infrastructure.Persistence;

namespace SWP391.Group2.Api.Controllers
{
    public class SnapshotsController : Controller
    {
        // =========================
        // #10: GET /api/groups/{groupId}/snapshots
        // Danh sách các lần đồng bộ (snapshots) theo group
        // =========================
        private readonly ApplicationDbContext _db;

        public SnapshotsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("{groupId:int}/snapshots")]
        public async Task<ActionResult<List<SnapshotListItemDto>>> GetSnapshots(int groupId)
        {

            var exists = await _db.Groups.AsNoTracking().AnyAsync(g => g.GroupId == groupId);
            if (!exists) return NotFound(new { message = $"Group {groupId} not found." });

            var data = await _db.Snapshots
                .AsNoTracking()
                .Where(s => s.SyncRun.Project.GroupId == groupId)
                .OrderByDescending(s => s.CapturedAt)
                .Select(s => new SnapshotListItemDto(
                    s.SnapshotId,
                    s.CapturedAt,
                    s.Label,
                    new SyncRunBriefDto(
                        s.SyncRun.SyncRunId,
                        s.SyncRun.ProjectId,
                        s.SyncRun.Project.ProjectName,
                        s.SyncRun.TriggerType,
                        s.SyncRun.ScopeType,
                        s.SyncRun.SprintId,
                        s.SyncRun.RunStatus,
                        s.SyncRun.StartedAt,
                        s.SyncRun.FinishedAt
                    )
                ))
                .Take(200)
                .ToListAsync();

            return Ok(data);
        }
    }
}
