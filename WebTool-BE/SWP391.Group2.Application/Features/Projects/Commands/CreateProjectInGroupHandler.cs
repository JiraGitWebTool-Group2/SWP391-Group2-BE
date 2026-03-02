using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Projects.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Projects.Commands
{
    public class CreateProjectInGroupHandler : IRequestHandler<CreateProjectInGroupCommand, ProjectDto>
    {
        private readonly IApplicationDbContext _db;

        public CreateProjectInGroupHandler(IApplicationDbContext db) => _db = db;

        public async Task<ProjectDto> Handle(CreateProjectInGroupCommand request, CancellationToken ct)
        {
            var name = (request.ProjectName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("ProjectName is required.");

            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == request.GroupId, ct);
            if (!groupExists)
                throw new KeyNotFoundException("Group not found.");

            //// Chống trùng tên trong cùng group (trả 409)
            //var nameExists = await _db.Projects.AnyAsync(p => p.GroupId == request.GroupId && p.ProjectName == name, ct);
            //if (nameExists)
            //    throw new InvalidOperationException("Project name already exists in this group.");

            var entity = new Project
            {
                GroupId = request.GroupId,
                ProjectName = name,
                JiraProjectKey = string.IsNullOrWhiteSpace(request.JiraProjectKey) ? null : request.JiraProjectKey.Trim(),
                GithubOrg = string.IsNullOrWhiteSpace(request.GithubOrg) ? null : request.GithubOrg.Trim(),
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _db.Projects.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new ProjectDto(
                entity.ProjectId, entity.GroupId, entity.ProjectName,
                entity.JiraProjectKey, entity.GithubOrg, entity.Description, entity.CreatedAt
            );
        }
    }
}