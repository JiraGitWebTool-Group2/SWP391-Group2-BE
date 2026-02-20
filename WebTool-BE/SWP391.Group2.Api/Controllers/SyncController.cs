using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.SyncRun.Commands;
using SWP391.Group2.Application.Features.SyncRun.Dtos;
using SWP391.Group2.Application.Features.SyncRun.Queries;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class SyncController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SyncController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("groups/{groupId:int}/sync")]
        public async Task<IActionResult> TriggerSync(
            int groupId,
            [FromBody] TriggerSyncDto dto)
        {
            var command = new TriggerSyncCommand
            {
                GroupId = groupId,
                TriggerType = dto.TriggerType,
                ScopeType = dto.ScopeType,
                SprintId = dto.SprintId,
                IncludeJira = dto.IncludeJira,
                IncludeGithub = dto.IncludeGithub
            };

            var syncId = await _mediator.Send(command);

            return Ok(new { syncId });
        }

        [HttpGet("sync/{syncId:int}")]
        public async Task<IActionResult> GetSyncDetail(int syncId)
        {
            var result = await _mediator.Send(new GetSyncDetailQuery(syncId));
            return Ok(result);
        }
    }
}

