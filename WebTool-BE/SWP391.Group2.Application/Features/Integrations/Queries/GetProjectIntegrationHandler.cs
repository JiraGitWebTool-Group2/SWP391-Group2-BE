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
    public class GetProjectIntegrationHandler : IRequestHandler<GetProjectIntegrationQuery, IntegrationDto>
    {
        private readonly IApplicationDbContext _db;

        public GetProjectIntegrationHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        //public async Task<ProjectIntegrationDto?> Handle(GetProjectIntegrationQuery request, CancellationToken cancellationToken)
        //{
        //    return await _db.Projects.AsNoTracking()
        //        .Where(p => p.ProjectId == request.ProjectId && p.GroupId == request.GroupId)
        //        .Select(p => new ProjectIntegrationDto(
        //            p.ProjectId,
        //            p.GroupId,
        //            p.ProjectName,
        //            p.JiraProjectKey,
        //            p.GithubOrg
        //        ))
        //        .FirstOrDefaultAsync(cancellationToken);
        //}

        public async Task<IntegrationDto> Handle(GetProjectIntegrationQuery request, CancellationToken ct)
    {
        var provider = NormalizeProvider(request.Provider);

        var entity = await _db.ProjectIntegrations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == request.ProjectId && x.Provider == provider, ct);

        if (entity is null)
            throw new KeyNotFoundException("Integration config not found.");

        return new IntegrationDto(
            entity.ProjectId,
            entity.Provider,
            entity.BaseUrl,
            entity.ProjectKey,
            entity.Org,
            !string.IsNullOrWhiteSpace(entity.TokenEncrypted),
            entity.UpdatedAt
        );
    }

    private static string NormalizeProvider(string provider)
    {
        var p = (provider ?? "").Trim().ToUpperInvariant();
        if (p is not ("JIRA" or "GITHUB"))
            throw new ArgumentException("Provider must be JIRA or GITHUB.");
        return p;
    }
    }
}
