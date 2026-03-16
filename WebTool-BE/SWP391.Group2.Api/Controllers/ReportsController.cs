using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Reports;
using SWP391.Group2.Application.Features.Reports.Commands;
using SWP391.Group2.Application.Features.Reports.Queries;
using System.Security.Claims;

namespace SWP391.Group2.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllReportsQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetReportByIdQuery(id), cancellationToken);

            if (result == null)
                return NotFound(new { message = $"Report with id = {id} was not found." });

            return Ok(result);
        }

        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateProgressReportRequest req, CancellationToken cancellationToken)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId))
                return Unauthorized(new { message = "Invalid user token." });

            try
            {
                var result = await _mediator.Send(new GenerateProgressReportCommand(
                    req.ProjectId,
                    req.StartDate,
                    req.EndDate,
                    req.Members,
                    req.ViewType,
                    userId
                ), cancellationToken);

                return Ok(new GenerateProgressReportResponse(
                    result.ReportId,
                    result.ProjectId,
                    result.CompletionRate,
                    result.DoneTasks,
                    result.InProgressTasks,
                    result.OverdueTasks,
                    result.GeneratedAt
                ));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
