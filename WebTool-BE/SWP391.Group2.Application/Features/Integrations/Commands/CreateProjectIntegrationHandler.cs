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

            ValidateByProvider(
                provider,
                request.BaseUrl,
                request.ProjectKey,
                request.Org);

            var projectExists = await _db.Projects
                .AnyAsync(p => p.ProjectId == request.ProjectId, ct);

            if (!projectExists)
                throw new ArgumentException("Project not found.");

            if (request.CreatedByUserId.HasValue)
            {
                var userExists = await _db.Users
                    .AnyAsync(u => u.UserId == request.CreatedByUserId.Value, ct);

                if (!userExists)
                    throw new ArgumentException("CreatedByUser not found.");
            }

            var exists = await _db.ProjectIntegrations
                .AnyAsync(x => x.ProjectId == request.ProjectId && x.Provider == provider, ct);

            if (exists)
                throw new InvalidOperationException("Integration already exists. Use PUT to update.");

            var now = DateTime.UtcNow;

            var entity = new ProjectIntegration
            {
                ProjectId = request.ProjectId,
                Provider = provider,
                BaseUrl = request.BaseUrl?.Trim(),
                ProjectKey = request.ProjectKey?.Trim(),
                Org = request.Org?.Trim(),
                CreatedByUserId = request.CreatedByUserId,
                LinkedAccount = request.LinkedAccount?.Trim(),
                VisibilityStatus = request.VisibilityStatus?.Trim(),
                LastVerifiedAt = request.LastVerifiedAt,
                VerificationNote = request.VerificationNote?.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            if (!string.IsNullOrWhiteSpace(request.Token))
                entity.TokenEncrypted = request.Token.Trim();

            _db.ProjectIntegrations.Add(entity);
            await _db.SaveChangesAsync(ct);

            return ToDto(entity);
        }

        private static string NormalizeProvider(string provider)
        {
            var p = (provider ?? string.Empty).Trim().ToUpperInvariant();

            if (p is not ("JIRA" or "GITHUB"))
                throw new ArgumentException("Provider must be JIRA or GITHUB.");

            return p;
        }

        private static void ValidateByProvider(
            string provider,
            string? baseUrl,
            string? projectKey,
            string? org)
        {
            if (provider == "JIRA")
            {
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new ArgumentException("Jira BaseUrl is required.");

                if (string.IsNullOrWhiteSpace(projectKey))
                    throw new ArgumentException("Jira ProjectKey is required.");
            }

            if (provider == "GITHUB")
            {
                if (string.IsNullOrWhiteSpace(org))
                    throw new ArgumentException("GitHub Org is required.");
            }
        }

        private static IntegrationDto ToDto(ProjectIntegration entity)
        {
            return new IntegrationDto(
                entity.IntegrationId,
                entity.ProjectId,
                entity.Provider,
                entity.BaseUrl,
                entity.ProjectKey,
                entity.Org,
                !string.IsNullOrWhiteSpace(entity.TokenEncrypted),
                entity.CreatedByUserId,
                entity.LinkedAccount,
                entity.VisibilityStatus,
                entity.LastVerifiedAt,
                entity.VerificationNote,
                entity.CreatedAt,
                entity.UpdatedAt
            );
        }
    }
}