using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Features.Groups.Commands;
using SWP391.Group2.Application.Features.Groups.Queries;
using SWP391.Group2.Domain.Entities;
using SWP391.Group2.Infrastructure.Persistence;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetGroupsQuery(), ct);
            return Ok(result);
        }

        public record CreateGroupRequest(string GroupName, string? Description);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _mediator.Send(new CreateGroupCommand(req.GroupName, req.Description), ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
