using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Auth.Queries
{
    public class GetMeHandler : IRequestHandler<GetMeQuery, MeDto>
    {
        private readonly IApplicationDbContext _db;

        public GetMeHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<MeDto> Handle(GetMeQuery request, CancellationToken ct)
        {
            var u = await _db.Users.AsNoTracking()
                .Where(x => x.UserId == request.UserId)
                .Select(x => new MeDto(x.UserId, x.Email, x.FullName, x.Provider, x.IsActive))
                .FirstOrDefaultAsync(ct);

            if (u is null) throw new UnauthorizedAccessException("User not found.");
            return u;
        }
    }
}
