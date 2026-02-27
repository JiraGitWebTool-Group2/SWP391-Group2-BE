using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Repositories.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Repositories.Command
{
    public class UpdateRepositoryHandler : IRequestHandler<UpdateRepositoryCommand, RepositoryDto>
    {
        private readonly IApplicationDbContext _db;

        public UpdateRepositoryHandler(IApplicationDbContext db) => _db = db;

        public async Task<RepositoryDto> Handle(UpdateRepositoryCommand request, CancellationToken ct)
        {
            var name = (request.RepoName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("RepoName is required.");

            var entity = await _db.Repositories
                .FirstOrDefaultAsync(r => r.RepoId == request.RepoId && r.ProjectId == request.ProjectId, ct);

            if (entity is null) throw new KeyNotFoundException("Repository not found.");

            // nếu đổi tên thì check unique
            var dup = await _db.Repositories.AnyAsync(r =>
                r.ProjectId == request.ProjectId &&
                r.RepoName == name &&
                r.RepoId != request.RepoId, ct);

            if (dup) throw new ArgumentException("Another repository with the same name already exists in this project.");

            entity.RepoName = name;
            entity.RepoUrl = request.RepoUrl?.Trim();
            entity.DefaultBranch = string.IsNullOrWhiteSpace(request.DefaultBranch) ? entity.DefaultBranch : request.DefaultBranch.Trim();

            await _db.SaveChangesAsync(ct);

            return new RepositoryDto(entity.RepoId, entity.ProjectId, entity.RepoName, entity.RepoUrl, entity.DefaultBranch, entity.CreatedAt);
        }
    }
}
