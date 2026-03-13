using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Integrations;
using SWP391.Group2.Application.Features.Integrations.Commands;
using SWP391.Group2.Application.Features.Integrations.Queries;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:int}/integrations")]
    public class ProjectIntegrationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectIntegrationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/projects/{projectId}/integrations/{provider}
        [Authorize]
        [HttpGet("{provider}")]
        public async Task<IActionResult> Get(int projectId, string provider, CancellationToken ct)
        {
            try
            {
                var dto = await _mediator.Send(new GetProjectIntegrationQuery(projectId, provider), ct);

                return Ok(ToApiDto(dto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT /api/projects/{projectId}/integrations
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Upsert(int projectId, [FromBody] UpsertIntegrationRequest req, CancellationToken ct)
        {
            try
            {
                var dto = await _mediator.Send(new UpsertProjectIntegrationCommand(
                    projectId,
                    req.Provider,
                    req.BaseUrl,
                    req.ProjectKey,
                    req.Org,
                    req.Token,
                    req.CreatedByUserId,
                    req.LinkedAccount,
                    req.VisibilityStatus,
                    req.LastVerifiedAt,
                    req.VerificationNote,
                    req.JiraStoryPointsFieldKey,
                    req.JiraSprintFieldKey,
                    req.JiraBoardId
                ), ct);

                return Ok(ToApiDto(dto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /api/projects/{projectId}/integrations
        // Create mới integration config. Nếu đã tồn tại: 409, dùng PUT để update.
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(int projectId, [FromBody] CreateIntegrationRequest req, CancellationToken ct)
        {
            try
            {
                var dto = await _mediator.Send(new CreateProjectIntegrationCommand(
                    projectId,
                    req.Provider,
                    req.BaseUrl,
                    req.ProjectKey,
                    req.Org,
                    req.Token,
                    req.CreatedByUserId,
                    req.LinkedAccount,
                    req.VisibilityStatus,
                    req.LastVerifiedAt,
                    req.VerificationNote,
                    req.JiraStoryPointsFieldKey,
                    req.JiraSprintFieldKey,
                    req.JiraBoardId
                ), ct);

                var apiDto = ToApiDto(dto);

                return CreatedAtAction(
                    nameof(Get),
                    new { projectId = dto.ProjectId, provider = dto.Provider },
                    apiDto);
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

        private static SWP391.Group2.Api.Contracts.Integrations.IntegrationDto ToApiDto(
            SWP391.Group2.Application.Features.Integrations.Dtos.IntegrationDto dto)
        {
            return new SWP391.Group2.Api.Contracts.Integrations.IntegrationDto(
                dto.IntegrationId,
                dto.ProjectId,
                dto.Provider,
                dto.BaseUrl,
                dto.ProjectKey,
                dto.Org,
                dto.HasToken,
                dto.CreatedByUserId,
                dto.LinkedAccount,
                dto.VisibilityStatus,
                dto.LastVerifiedAt,
                dto.VerificationNote,
                dto.JiraStoryPointsFieldKey,
                dto.JiraSprintFieldKey,
                dto.JiraBoardId,
                dto.LastJiraSyncAt,
                dto.CreatedAt,
                dto.UpdatedAt
            );
        }
    }
}
