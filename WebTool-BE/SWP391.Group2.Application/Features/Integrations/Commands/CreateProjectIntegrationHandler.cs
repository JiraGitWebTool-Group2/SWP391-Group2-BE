using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Integrations.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Integrations.Commands
{
    public class CreateProjectIntegrationHandler : IRequestHandler<CreateProjectIntegrationCommand, IntegrationDto>
    {
        private readonly IApplicationDbContext _db;

        public CreateProjectIntegrationHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IntegrationDto> Handle(CreateProjectIntegrationCommand request, CancellationToken ct)
        {
            var provider = NormalizeProvider(request.Provider);

            // Validate tối thiểu theo provider
            if (provider == "JIRA")
            {
                if (string.IsNullOrWhiteSpace(request.BaseUrl))
                    throw new ArgumentException("Jira BaseUrl is required.");
                if (string.IsNullOrWhiteSpace(request.ProjectKey))
                    throw new ArgumentException("Jira ProjectKey is required.");
            }

            if (provider == "GITHUB")
            {
                if (string.IsNullOrWhiteSpace(request.Org))
                    throw new ArgumentException("GitHub Org is required.");
            }

            // check project exists (đỡ bị insert lạc trôi)
            var projectExists = await _db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId, ct);
            if (!projectExists)
                throw new ArgumentException("Project not found.");

            // Không create nếu đã tồn tại (khác PUT upsert)
            var exists = await _db.ProjectIntegrations
                .AnyAsync(x => x.ProjectId == request.ProjectId && x.Provider == provider, ct);

            if (exists)
                throw new InvalidOperationException("Integration already exists. Use PUT to update.");

            var entity = new ProjectIntegration
            {
                ProjectId = request.ProjectId,
                Provider = provider,
                BaseUrl = request.BaseUrl?.Trim(),
                ProjectKey = request.ProjectKey?.Trim(),
                Org = request.Org?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Demo: lưu plain vào token_encrypted. Sau này bạn thay bằng encrypt/hashing.
            if (!string.IsNullOrWhiteSpace(request.Token))
                entity.TokenEncrypted = request.Token.Trim();

            _db.ProjectIntegrations.Add(entity);
            await _db.SaveChangesAsync(ct);

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