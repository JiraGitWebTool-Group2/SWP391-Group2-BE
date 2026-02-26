using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Abstractions.Auth;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Auth.Command
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, TokenPair>
    {
        private readonly IApplicationDbContext _db;
        private readonly ITokenService _tokens;

        public RefreshTokenHandler(IApplicationDbContext db, ITokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        public async Task<TokenPair> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ArgumentException("RefreshToken is required.");

            var hash = _tokens.HashRefreshToken(request.RefreshToken);

            var rt = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

            if (rt is null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (rt.RevokedAt is not null)
                throw new UnauthorizedAccessException("Refresh token revoked.");

            if (rt.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token expired.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == rt.UserId, ct);
            if (user is null || !user.IsActive)
                throw new UnauthorizedAccessException("User not found or inactive.");

            // ROTATION: revoke old token
            rt.RevokedAt = DateTime.UtcNow;

            // Issue new pair
            var newAccess = _tokens.CreateAccessToken(user);
            var newRefresh = _tokens.GenerateRefreshToken();
            var newHash = _tokens.HashRefreshToken(newRefresh);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(_tokens.GetRefreshTokenDays())
                // CreatedAt: DB default SYSDATETIME() sẽ tự set
            });

            await _db.SaveChangesAsync(ct);

            return new TokenPair(newAccess, newRefresh);
        }
    }
}
