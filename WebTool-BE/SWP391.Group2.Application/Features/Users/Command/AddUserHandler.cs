using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Users.Dtos;
using SWP391.Group2.Domain.Entities;
using System;


namespace SWP391.Group2.Application.Features.Users.Command
{
    public class AddUserHandler : IRequestHandler<AddUserCommand, UserDto>
    {
        private readonly IApplicationDbContext _db; // đổi theo DbContext thật của bạn
        private readonly PasswordHasher<User> _hasher = new();

        public AddUserHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserDto> Handle(AddUserCommand cmd, CancellationToken ct)
        {
            var req = cmd.Request;

            var email = req.Email.Trim().ToLowerInvariant();
            var fullName = req.FullName.Trim();
            var role = string.IsNullOrWhiteSpace(req.Role)
                ? "STUDENT"
                : req.Role.Trim().ToUpperInvariant();

            if (role is not ("ADMIN" or "LECTURER" or "STUDENT"))
                throw new ArgumentException("Role không hợp lệ. Chỉ nhận ADMIN/LECTURER/STUDENT.");
            var provider = string.IsNullOrWhiteSpace(req.Provider) ? "LOCAL" : req.Provider.Trim().ToUpperInvariant();
            var providerUserId = string.IsNullOrWhiteSpace(req.ProviderUserId) ? null : req.ProviderUserId.Trim();

            if (provider is not ("LOCAL" or "GOOGLE" or "GITHUB"))
                throw new ArgumentException("Provider không hợp lệ. Chỉ nhận LOCAL/GOOGLE/GITHUB.");

            if (provider == "LOCAL")
            {
                if (string.IsNullOrWhiteSpace(req.Password))
                    throw new ArgumentException("Provider LOCAL yêu cầu Password.");

                // LOCAL thì thường không cần ProviderUserId
                providerUserId = null;
            }
            else
            {
                // GOOGLE/GITHUB thì nên có ProviderUserId
                if (string.IsNullOrWhiteSpace(providerUserId))
                    throw new ArgumentException("Provider GOOGLE/GITHUB yêu cầu ProviderUserId.");
            }

            var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == email, ct);
            if (exists)
                throw new InvalidOperationException("Email đã tồn tại.");

            var now = DateTime.UtcNow;

            var user = new User
            {
                Email = email,
                FullName = fullName,
                Provider = provider,
                ProviderUserId = providerUserId,
                System_Role = role, // THÊM DÒNG NÀY
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (provider == "LOCAL")
            {
                user.PasswordHash = _hasher.HashPassword(user, req.Password!);
            }

            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // tránh race-condition unique email
                throw new InvalidOperationException("Không thể tạo user (có thể email đã tồn tại).");
            }

            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Provider = user.Provider,
                ProviderUserId = user.ProviderUserId,
                System_Role = user.System_Role, // THÊM
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
