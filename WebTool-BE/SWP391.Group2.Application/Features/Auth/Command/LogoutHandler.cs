using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Abstractions.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Auth.Command
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly IApplicationDbContext _db;
        private readonly ITokenService _tokens;

        public LogoutHandler(IApplicationDbContext db, ITokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ArgumentException("RefreshToken is required.");

            var hash = _tokens.HashRefreshToken(request.RefreshToken);

            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

            // Idempotent logout: token không tồn tại thì coi như logout xong
            if (rt is not null && rt.RevokedAt is null)
            {
                rt.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }

            return Unit.Value;
        }
    }
}
