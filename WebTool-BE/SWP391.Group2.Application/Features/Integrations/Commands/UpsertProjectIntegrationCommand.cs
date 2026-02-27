using MediatR;
using SWP391.Group2.Application.Features.Integrations.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Integrations.Commands
{
    public record UpsertProjectIntegrationCommand(
        int ProjectId,
        string Provider,
        string? BaseUrl,
        string? ProjectKey,
        string? Org,
        string? Token
    ) : IRequest<IntegrationDto>;
}
