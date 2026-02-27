using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Repositories.Dtos;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Repositories.Command
{
    public class CreateRepositoryHandler : IRequestHandler<CreateRepositoryCommand, RepositoryDto>
    {
        private readonly IApplicationDbContext _db;

        public CreateRepositoryHandler(IApplicationDbContext db) => _db = db;

        public async Task<RepositoryDto> Handle(CreateRepositoryCommand request, CancellationToken ct)
        {
            var name = (request.RepoName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("RepoName is required.");

            // check project exists (optional but recommended)
            var projectExists = await _db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId, ct);
            if (!projectExists) throw new ArgumentException("Project not found.");

            // unique (project_id, repo_name)
            var exists = await _db.Repositories.AnyAsync(r => r.ProjectId == request.ProjectId && r.RepoName == name, ct);
            if (exists) throw new ArgumentException("Repository already exists in this project.");

            var entity = new Repository
            {
                ProjectId = request.ProjectId,
                RepoName = name,
                RepoUrl = request.RepoUrl?.Trim(),
                DefaultBranch = string.IsNullOrWhiteSpace(request.DefaultBranch) ? "main" : request.DefaultBranch.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Repositories.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new RepositoryDto(entity.RepoId, entity.ProjectId, entity.RepoName, entity.RepoUrl, entity.DefaultBranch, entity.CreatedAt);
        }
    }
}
