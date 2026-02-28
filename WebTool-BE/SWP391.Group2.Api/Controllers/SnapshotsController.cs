using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    //[HttpGet("{groupId:int}/snapshots")]
    //public async Task<ActionResult<List<SnapshotListItemDto>>> GetSnapshots(int groupId)
    //{
    //    try
    //    {
    //        var data = await _mediator.Send(new GetGroupSnapshotsQuery(groupId));
    //        return Ok(data);
    //    }
    //    catch (KeyNotFoundException ex)
    //    {
    //        return NotFound(new { message = ex.Message });
    //    }
    //}


    [Authorize]
    [HttpGet("{snapshotId:int}/commits")]
    public async Task<IActionResult> GetCommits(int snapshotId, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetSnapshotCommitsQuery(snapshotId), ct);

            // map App DTO -> Api DTO
            var dto = result.Select(x => new SnapshotCommitItemDto(
                x.CommitId,
                x.CommitHash,
                x.Message,
                x.CommittedAt,
                x.CommitUrl,
                x.RepoId,
                x.RepoName
            ));

            return Ok(dto);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    [Authorize]
    [HttpGet("{snapshotId:int}/repos-summary")]
    public async Task<IActionResult> GetRepoSummary(int snapshotId, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetSnapshotRepoSummaryQuery(snapshotId), ct);

            var dto = result.Select(x => new SnapshotRepoSummaryDto(
                x.RepoId,
                x.RepoName,
                x.CommitCount
            ));

            return Ok(dto);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }
}