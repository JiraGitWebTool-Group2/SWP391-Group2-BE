using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.Lecturers.Queries;
using LecturerResponseDto = SWP391.Group2.Api.Contracts.Lecturers.LecturerDto;

namespace SWP391.Group2.Api.Controllers;

[ApiController]
[Route("api/lecturers")]
public class LecturersController : ControllerBase
{
    private readonly IMediator _mediator;

    public LecturersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetLecturers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLecturersQuery(), cancellationToken);

        var response = result.Select(x => new LecturerResponseDto
        {
            LecturerId = x.LecturerId,
            Email = x.Email,
            FullName = x.FullName,
            SystemRole = x.SystemRole,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });

        return Ok(response);
    }

    [HttpGet("{lecturerId}")]
    public async Task<IActionResult> GetLecturerDetail(
    int lecturerId,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLecturerDetailQuery(lecturerId),
            cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{lecturerId}/classes")]
    public async Task<IActionResult> GetLecturerClasses(
        int lecturerId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLecturerClassesQuery(lecturerId),
            cancellationToken);

        return Ok(result);
    }
}