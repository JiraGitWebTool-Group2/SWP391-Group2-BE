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
    public class DevLoginHandler : IRequestHandler<DevLoginCommand, TokenPair>
    {
        private readonly IApplicationDbContext _db;
        private readonly ITokenService _tokens;

        public DevLoginHandler(IApplicationDbContext db, ITokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        public async Task<TokenPair> Handle(DevLoginCommand request, CancellationToken ct)
        {
            var email = (request.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email, ct);
            if (user is null) throw new UnauthorizedAccessException("Email not in whitelist.");
            if (!user.IsActive) throw new UnauthorizedAccessException("User inactive.");

            var access = _tokens.CreateAccessToken(user);
            var refresh = _tokens.GenerateRefreshToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = _tokens.HashRefreshToken(refresh),
                ExpiresAt = DateTime.UtcNow.AddDays(_tokens.GetRefreshTokenDays())
            });

            await _db.SaveChangesAsync(ct);

            return new TokenPair(access, refresh);
        }
    }
}
