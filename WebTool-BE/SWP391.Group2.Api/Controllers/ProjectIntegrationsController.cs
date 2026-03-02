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

                // map Application DTO -> Api DTO
                return Ok(new Contracts.Integrations.IntegrationDto(
                    dto.ProjectId, dto.Provider, dto.BaseUrl, dto.ProjectKey, dto.Org, dto.HasToken, dto.UpdatedAt
                ));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
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
                    req.Token
                ), ct);

                return Ok(new Contracts.Integrations.IntegrationDto(
                    dto.ProjectId, dto.Provider, dto.BaseUrl, dto.ProjectKey, dto.Org, dto.HasToken, dto.UpdatedAt
                ));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
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
                    req.Token
                ), ct);

                var apiDto = new Contracts.Integrations.IntegrationDto(
                    dto.ProjectId, dto.Provider, dto.BaseUrl, dto.ProjectKey, dto.Org, dto.HasToken, dto.UpdatedAt
                );

                // Trả 201 + Location trỏ về endpoint GET
                return CreatedAtAction(nameof(Get), new { projectId = dto.ProjectId, provider = dto.Provider }, apiDto);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }
    }
}
