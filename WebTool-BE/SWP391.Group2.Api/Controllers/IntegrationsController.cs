using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Infrastructure.Persistence;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/groups/{groupId:int}/projects/{projectId:int}/integrations")]
    public class ProjectIntegrationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProjectIntegrationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public record UpdateIntegrationRequest(
            string? JiraProjectKey,
            string? GithubOrg
        );

        // PUT /api/groups/{groupId}/projects/{projectId}/integrations
        [HttpPut]
        public async Task<IActionResult> Update(int groupId, int projectId, [FromBody] UpdateIntegrationRequest req)
        {
            // Check group tồn tại
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists) return NotFound("Group not found.");

            // Lấy project đúng group
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.GroupId == groupId);

            if (project is null) return NotFound("Project not found in this group.");

            // Update integration fields
            project.JiraProjectKey = req.JiraProjectKey;
            project.GithubOrg = req.GithubOrg;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                project.ProjectId,
                project.GroupId,
                project.ProjectName,
                project.JiraProjectKey,
                project.GithubOrg
            });
        }

        // GET /api/groups/{groupId}/projects/{projectId}/integrations
        [HttpGet]
        public async Task<IActionResult> Get(int groupId, int projectId)
        {
            var project = await _db.Projects.AsNoTracking()
                .Where(p => p.ProjectId == projectId && p.GroupId == groupId)
                .Select(p => new
                {
                    p.ProjectId,
                    p.GroupId,
                    p.ProjectName,
                    p.JiraProjectKey,
                    p.GithubOrg
                })
                .FirstOrDefaultAsync();

            if (project is null) return NotFound("Project not found in this group.");
            return Ok(project);
        }
    }
}
