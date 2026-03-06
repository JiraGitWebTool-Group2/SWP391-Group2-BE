using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.Reports.Queries;

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
    }
}
