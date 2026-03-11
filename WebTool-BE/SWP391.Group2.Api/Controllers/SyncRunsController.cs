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

                return Ok(new StartSyncResponse(
                    syncRunId,
                    req.ProjectId,
                    "RUNNING"
                ));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{syncRunId:int}")]
        public async Task<IActionResult> GetStatus(int syncRunId, CancellationToken ct)
        {
            var res = await _mediator.Send(new GetSyncRunStatusQuery(syncRunId), ct);

            if (res is null)
                return NotFound("Sync run not found.");

            return Ok(res);
        }
    }
}
