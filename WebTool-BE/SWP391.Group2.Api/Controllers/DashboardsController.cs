using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.Dashboards.Dtos;
using SWP391.Group2.Application.Features.Dashboards.Queries;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // #11: GET /api/groups/{groupId}/dashboard
        [HttpGet("{groupId:int}/dashboard")]
        public async Task<ActionResult<DashboardDto>> GetDashboard(int groupId)
        {
            try
            {
                var dto = await _mediator.Send(new GetDashboardQuery(groupId));
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}