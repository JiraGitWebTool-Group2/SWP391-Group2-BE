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
    public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, TokenPair>
    {
        private readonly IApplicationDbContext _db;
        private readonly IGoogleTokenValidator _google;
        private readonly ITokenService _tokens;

        public GoogleLoginHandler(
            IApplicationDbContext db,
            IGoogleTokenValidator google,
            ITokenService tokens)
        {
            _db = db;
            _google = google;
            _tokens = tokens;
        }

        public async Task<TokenPair> Handle(GoogleLoginCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
                throw new ArgumentException("IdToken is required.");

            var googleUser = await _google.ValidateAsync(request.IdToken, ct);

            var emailLower = googleUser.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);
            if (user is null)
                throw new UnauthorizedAccessException("Email not in whitelist.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User inactive.");

            // Optional: update provider + name (nhẹ nhàng)
            user.Provider ??= "GOOGLE";
            if (!string.IsNullOrWhiteSpace(googleUser.FullName) && string.IsNullOrWhiteSpace(user.FullName))
                user.FullName = googleUser.FullName;

            // Create tokens
            var accessToken = _tokens.CreateAccessToken(user);
            var refreshToken = _tokens.GenerateRefreshToken();
            var pair = new TokenPair(accessToken, refreshToken);

            // Store refresh token hash
            var refreshHash = _tokens.HashRefreshToken(pair.RefreshToken);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = refreshHash,
                ExpiresAt = DateTime.UtcNow.AddDays(14) // lát nữa đưa ra config
            });

            await _db.SaveChangesAsync(ct);

            return pair;
        }
    }
}
