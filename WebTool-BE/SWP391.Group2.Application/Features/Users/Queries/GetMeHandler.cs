using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions; // IApplicationDbContext của bạn
using SWP391.Group2.Application.Features.Users.Dtos;

namespace SWP391.Group2.Application.Features.Users.Queries
{
    public class GetMeHandler : IRequestHandler<GetMeQuery, UserDto>
    {
        private readonly IApplicationDbContext _db;

        public GetMeHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email, ct);

            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy user trong database.");

            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Provider = user.Provider,
                ProviderUserId = user.ProviderUserId,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}