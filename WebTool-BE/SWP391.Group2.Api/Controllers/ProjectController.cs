using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Projects;
using SWP391.Group2.Api.Contracts.Projects.SWP391.Group2.Api.Contracts.Projects;
using SWP391.Group2.Application.Features.Projects.Commands;
using SWP391.Group2.Application.Features.Projects.Queries;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/groups/{groupId:int}/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjectController(IMediator mediator) => _mediator = mediator;

        // GET /api/groups/{groupId}/projects
        [HttpGet]
        public async Task<IActionResult> GetByGroup(int groupId, CancellationToken ct)
        {
            try
            {
                var list = await _mediator.Send(new GetGroupProjectsQuery(groupId), ct);

                var res = list.Select(p => new ProjectDto(
                    p.ProjectId, p.GroupId, p.ProjectName,
                    p.JiraProjectKey, p.GithubOrg, p.Description, p.CreatedAt
                ));

                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST /api/groups/{groupId}/projects
        // Có [Authorize] giống style các POST quan trọng khác trong hệ.
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(int groupId, [FromBody] CreateProjectRequest req, CancellationToken ct)
        {
            try
            {
                var dto = await _mediator.Send(new CreateProjectInGroupCommand(
                    groupId, req.ProjectName, req.JiraProjectKey, req.GithubOrg, req.Description
                ), ct);

                var apiDto = new ProjectDto(
                    dto.ProjectId, dto.GroupId, dto.ProjectName,
                    dto.JiraProjectKey, dto.GithubOrg, dto.Description, dto.CreatedAt
                );

                return StatusCode(StatusCodes.Status201Created, apiDto);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }
    }
}