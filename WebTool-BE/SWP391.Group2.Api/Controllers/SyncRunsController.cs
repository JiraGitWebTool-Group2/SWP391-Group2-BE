using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Sync;
using SWP391.Group2.Application.Features.Sync.Command;
using SWP391.Group2.Application.Features.Sync.Queries;
using System.Security.Claims;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/sync-runs")]
    public class SyncRunsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SyncRunsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Start([FromBody] StartSyncRequest req, CancellationToken ct)
        {
            // tạm: lấy userId từ JWT (để triggered_by_user_id)
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = int.TryParse(idStr, out var uid) ? uid : null;

            try
            {
                var syncRunId = await _mediator.Send(new StartSyncCommand(
                    req.ProjectId,
                    req.IncludeJira,
                    req.IncludeGithub,
                    req.ScopeType,
                    req.SprintId,
                    userId,
                    "MANUAL"
                ), ct);

                return Ok(new StartSyncResponse(syncRunId, "RUNNING"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{syncRunId:int}")]
        public async Task<IActionResult> GetStatus(int syncRunId, CancellationToken ct)
        {
            try
            {
                var res = await _mediator.Send(new GetSyncRunStatusQuery(syncRunId), ct);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
