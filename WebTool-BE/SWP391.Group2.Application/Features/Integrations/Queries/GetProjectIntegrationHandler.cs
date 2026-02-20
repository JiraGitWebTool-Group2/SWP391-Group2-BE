using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Integrations.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Integrations.Queries
{
    public class GetProjectIntegrationHandler : IRequestHandler<GetProjectIntegrationQuery, ProjectIntegrationDto?>
    {
        private readonly IApplicationDbContext _db;

        public GetProjectIntegrationHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ProjectIntegrationDto?> Handle(GetProjectIntegrationQuery request, CancellationToken cancellationToken)
        {
            return await _db.Projects.AsNoTracking()
                .Where(p => p.ProjectId == request.ProjectId && p.GroupId == request.GroupId)
                .Select(p => new ProjectIntegrationDto(
                    p.ProjectId,
                    p.GroupId,
                    p.ProjectName,
                    p.JiraProjectKey,
                    p.GithubOrg
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
