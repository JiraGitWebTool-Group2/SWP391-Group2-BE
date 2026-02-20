using MediatR;
using SWP391.Group2.Application.Features.Integrations.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Integrations.Queries
{
    public record GetProjectIntegrationQuery(int GroupId, int ProjectId) : IRequest<ProjectIntegrationDto?>;
}
