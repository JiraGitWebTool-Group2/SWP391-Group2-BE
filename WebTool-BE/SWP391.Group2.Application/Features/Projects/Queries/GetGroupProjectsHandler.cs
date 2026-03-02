using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Projects.Dtos;

namespace SWP391.Group2.Application.Features.Projects.Queries
{
    public class GetGroupProjectsHandler : IRequestHandler<GetGroupProjectsQuery, List<ProjectDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetGroupProjectsHandler(IApplicationDbContext db) => _db = db;

        public async Task<List<ProjectDto>> Handle(GetGroupProjectsQuery request, CancellationToken ct)
        {
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == request.GroupId, ct);
            if (!groupExists) throw new KeyNotFoundException("Group not found.");

            return await _db.Projects.AsNoTracking()
                .Where(p => p.GroupId == request.GroupId)
                .OrderByDescending(p => p.ProjectId)
                .Select(p => new ProjectDto(
                    p.ProjectId, p.GroupId, p.ProjectName,
                    p.JiraProjectKey, p.GithubOrg, p.Description, p.CreatedAt
                ))
                .ToListAsync(ct);
        }
    }
}