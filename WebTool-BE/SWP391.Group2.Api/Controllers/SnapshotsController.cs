using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.Snapshots.Dtos;
using SWP391.Group2.Application.Features.Snapshots.Queries;


namespace SWP391.Group2.Api.Controllers;

[ApiController]
[Route("api/groups")]
public class SnapshotsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SnapshotsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // #10: GET /api/groups/{groupId}/snapshots
    [HttpGet("{groupId:int}/snapshots")]
    public async Task<ActionResult<List<SnapshotListItemDto>>> GetSnapshots(int groupId)
    {
        try
        {
            var data = await _mediator.Send(new GetGroupSnapshotsQuery(groupId));
            return Ok(data);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}