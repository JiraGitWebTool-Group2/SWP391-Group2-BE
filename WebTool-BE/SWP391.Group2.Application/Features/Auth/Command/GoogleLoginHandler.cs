using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Abstractions.Auth;
using SWP391.Group2.Domain.Entities;

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

            // Validate Google token
            var googleUser = await _google.ValidateAsync(request.IdToken, ct);

            // Validate role
            var role = request.Role?.Trim().ToUpper() ?? "STUDENT";

            if (role != "ADMIN" && role != "LECTURER" && role != "STUDENT")
                throw new ArgumentException("Invalid role.");

            var emailLower = googleUser.Email.Trim().ToLowerInvariant();

            // Find user
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, ct);

            // Create user if not exist
            if (user is null)
            {
                user = new User
                {
                    Email = emailLower,
                    FullName = googleUser.FullName,
                    Provider = "GOOGLE",
                    System_Role = role,
                    IsActive = true
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync(ct);
            }

            if (user.System_Role != role)
                throw new UnauthorizedAccessException("Role mismatch.");

            // Update provider if missing
            user.Provider ??= "GOOGLE";

            if (!string.IsNullOrWhiteSpace(googleUser.FullName) && string.IsNullOrWhiteSpace(user.FullName))
                user.FullName = googleUser.FullName;

            // Create tokens
            var accessToken = _tokens.CreateAccessToken(user);
            var refreshToken = _tokens.GenerateRefreshToken();
            var pair = new TokenPair(accessToken, refreshToken);

            // Store refresh token
            var refreshHash = _tokens.HashRefreshToken(pair.RefreshToken);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = refreshHash,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            });

            await _db.SaveChangesAsync(ct);

            return pair;
        }
    }
}