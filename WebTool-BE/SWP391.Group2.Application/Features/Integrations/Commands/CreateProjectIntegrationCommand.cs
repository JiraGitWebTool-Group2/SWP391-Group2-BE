using MediatR;
using SWP391.Group2.Application.Features.Integrations.Dtos;

namespace SWP391.Group2.Application.Features.Integrations.Commands
{
    // POST: create mới integration config (không upsert)
    public record CreateProjectIntegrationCommand(
        int ProjectId,
        string Provider,
        string? BaseUrl,
        string? ProjectKey,
        string? Org,
        string? Token
    ) : IRequest<IntegrationDto>;
}