using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Repositories;
using SWP391.Group2.Application.Features.Repositories.Command;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:int}/repositories")]
    public class ProjectRepositoriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjectRepositoriesController(IMediator mediator) => _mediator = mediator;

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(int projectId, [FromBody] CreateRepositoryRequest req, CancellationToken ct)
        {
            try
            {
                var dto = await _mediator.Send(new CreateRepositoryCommand(projectId, req.RepoName, req.RepoUrl, req.DefaultBranch), ct);
                return Ok(new RepositoryDto(dto.RepoId, dto.ProjectId, dto.RepoName, dto.RepoUrl, dto.DefaultBranch, dto.CreatedAt));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [Authorize]
        [HttpPut("{repoId:int}")]
        public async Task<IActionResult> Update(int projectId, int repoId, [FromBody] UpdateRepositoryRequest req, CancellationToken ct)
        {
            try
            {
                var dto = await _mediator.Send(new UpdateRepositoryCommand(projectId, repoId, req.RepoName, req.RepoUrl, req.DefaultBranch), ct);
                return Ok(new RepositoryDto(dto.RepoId, dto.ProjectId, dto.RepoName, dto.RepoUrl, dto.DefaultBranch, dto.CreatedAt));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }
    }
}
