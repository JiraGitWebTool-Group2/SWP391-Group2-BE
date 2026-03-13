using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Integrations.Dtos;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Integrations.Commands
{
    public class UpsertProjectIntegrationHandler : IRequestHandler<UpsertProjectIntegrationCommand, IntegrationDto>
    {
        private readonly IApplicationDbContext _db;

        public UpsertProjectIntegrationHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IntegrationDto> Handle(UpsertProjectIntegrationCommand request, CancellationToken ct)
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

            var entity = await _db.ProjectIntegrations
                .FirstOrDefaultAsync(
                    x => x.ProjectId == request.ProjectId && x.Provider == provider,
                    ct);

            var now = DateTime.UtcNow;

            if (entity is null)
            {
                entity = new ProjectIntegration
                {
                    ProjectId = request.ProjectId,
                    Provider = provider,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _db.ProjectIntegrations.Add(entity);
            }

            entity.BaseUrl = request.BaseUrl?.Trim();
            entity.ProjectKey = request.ProjectKey?.Trim();
            entity.Org = request.Org?.Trim();
            entity.CreatedByUserId = request.CreatedByUserId;
            entity.LinkedAccount = request.LinkedAccount?.Trim();
            entity.VisibilityStatus = request.VisibilityStatus?.Trim();
            entity.LastVerifiedAt = request.LastVerifiedAt;
            entity.VerificationNote = request.VerificationNote?.Trim();

            if (!string.IsNullOrWhiteSpace(request.Token))
                entity.TokenEncrypted = request.Token.Trim();

            entity.UpdatedAt = now;

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
                entity.JiraStoryPointsFieldKey,
                entity.JiraSprintFieldKey,
                entity.JiraBoardId,
                entity.LastJiraSyncAt,
                entity.CreatedAt,
                entity.UpdatedAt
            );
        }
    }
}
