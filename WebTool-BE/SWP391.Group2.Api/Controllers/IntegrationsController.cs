using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Features.Integrations.Commands;
using SWP391.Group2.Application.Features.Integrations.Queries;
using SWP391.Group2.Infrastructure.Persistence;

namespace SWP391.Group2.Api.Controllers
{
    //[ApiController]
    //[Route("api/groups/{groupId:int}/projects/{projectId:int}/integrations")]
    //public class ProjectIntegrationsController : ControllerBase
    //{
    //    private readonly IMediator _mediator;

    //    public ProjectIntegrationsController(IMediator mediator)
    //    {
    //        _mediator = mediator;
    //    }

    //    public record UpdateIntegrationRequest(string? JiraProjectKey, string? GithubOrg);

    //    [HttpGet]
    //    public async Task<IActionResult> Get(int groupId, int projectId, CancellationToken ct)
    //    {
    //        var dto = await _mediator.Send(new GetProjectIntegrationQuery(groupId, projectId), ct);
    //        if (dto is null) return NotFound("Project not found in this group.");
    //        return Ok(dto);
    //    }

    //    [HttpPut]
    //    public async Task<IActionResult> Update(int groupId, int projectId, [FromBody] UpdateIntegrationRequest req, CancellationToken ct)
    //    {
    //        var dto = await _mediator.Send(
    //            new UpdateProjectIntegrationCommand(groupId, projectId, req.JiraProjectKey, req.GithubOrg),
    //            ct);

    //        if (dto is null) return NotFound("Group/Project not found.");
    //        return Ok(dto);
    //    }
    //}
}
