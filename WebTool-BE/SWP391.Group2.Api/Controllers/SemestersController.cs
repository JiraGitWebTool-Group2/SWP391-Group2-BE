using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Semesters;
using SWP391.Group2.Application.Features.Semesters.Commands;
using SWP391.Group2.Application.Features.Semesters.Queries;

namespace SWP391.Group2.Api.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SemestersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSemester(
        [FromBody] CreateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateSemesterCommand(
                request.Code,
                request.Name,
                request.StartDate,
                request.EndDate,
                request.Status),
            cancellationToken);

        var response = new SemesterDto
        {
            SemesterId = result.SemesterId,
            Code = result.Code,
            Name = result.Name,
            StartDate = result.StartDate,
            EndDate = result.EndDate,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return CreatedAtAction(nameof(GetSemesterById), new { id = response.SemesterId }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetSemesters(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSemestersQuery(), cancellationToken);

        var response = result.Select(x => new SemesterDto
        {
            SemesterId = x.SemesterId,
            Code = x.Code,
            Name = x.Name,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSemesterById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSemesterByIdQuery(id), cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Semester with id {id} was not found." });

        var response = new SemesterDto
        {
            SemesterId = result.SemesterId,
            Code = result.Code,
            Name = result.Name,
            StartDate = result.StartDate,
            EndDate = result.EndDate,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSemester(
        int id,
        [FromBody] UpdateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateSemesterCommand(
                id,
                request.Code,
                request.Name,
                request.StartDate,
                request.EndDate,
                request.Status),
            cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Semester with id {id} was not found." });

        var response = new SemesterDto
        {
            SemesterId = result.SemesterId,
            Code = result.Code,
            Name = result.Name,
            StartDate = result.StartDate,
            EndDate = result.EndDate,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSemester(int id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteSemesterCommand(id), cancellationToken);

        if (!deleted)
            return NotFound(new { message = $"Semester with id {id} was not found." });

        return NoContent();
    }
}