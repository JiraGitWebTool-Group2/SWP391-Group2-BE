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

            var entity = await _db.ProjectIntegrations
                .FirstOrDefaultAsync(x => x.ProjectId == request.ProjectId && x.Provider == provider, ct);

            if (entity is null)
            {
                entity = new ProjectIntegration
                {
                    ProjectId = request.ProjectId,
                    Provider = provider,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.ProjectIntegrations.Add(entity);
            }

            entity.BaseUrl = request.BaseUrl?.Trim();
            entity.ProjectKey = request.ProjectKey?.Trim();
            entity.Org = request.Org?.Trim();

            // Demo: lưu plain vào token_encrypted. Sau này bạn thay bằng encrypt/hashing.
            if (!string.IsNullOrWhiteSpace(request.Token))
                entity.TokenEncrypted = request.Token.Trim();

            entity.UpdatedAt = DateTime.UtcNow;

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
