using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWP391.Group2.Domain.Entities;
using SWP391.Group2.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public record LoginRequest(string Email, string Password);
        public record RefreshRequest(string RefreshToken);
        public record LogoutRequest(string RefreshToken);

        public record GoogleLoginRequest(string IdToken);

        public record TokenResponse(string AccessToken, string RefreshToken);
        public record MeResponse(int UserId, string Email, string FullName, string? Provider);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user is null) return Unauthorized("Invalid credentials.");
            if (!user.IsActive) return Unauthorized("User is inactive.");
            if (!string.Equals(user.Provider, "LOCAL", StringComparison.OrdinalIgnoreCase))
                return Unauthorized("This account uses an external provider.");
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return Unauthorized("Password not set.");

            var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
            if (!ok) return Unauthorized("Invalid credentials.");

            var accessToken = CreateJwt(user);

            var refreshToken = GenerateRefreshToken();
            var refreshHash = Sha256Hex(refreshToken);

            var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "14");

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = refreshHash,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
            });

            await _db.SaveChangesAsync();

            return Ok(new TokenResponse(accessToken, refreshToken));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
                return Unauthorized("Missing refresh token.");

            var oldHash = Sha256Hex(req.RefreshToken);

            var tokenRow = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == oldHash);
            if (tokenRow is null) return Unauthorized("Invalid refresh token.");
            if (tokenRow.RevokedAt is not null) return Unauthorized("Refresh token revoked.");
            if (tokenRow.ExpiresAt <= DateTime.UtcNow) return Unauthorized("Refresh token expired.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == tokenRow.UserId);
            if (user is null || !user.IsActive) return Unauthorized("User not found/inactive.");

            // Rotation: revoke old, issue new
            tokenRow.RevokedAt = DateTime.UtcNow;

            var newRefresh = GenerateRefreshToken();
            var newHash = Sha256Hex(newRefresh);

            var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "14");

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
            });

            var newAccess = CreateJwt(user);

            await _db.SaveChangesAsync();

            return Ok(new TokenResponse(newAccess, newRefresh));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
                return Ok(); // idempotent

            var hash = Sha256Hex(req.RefreshToken);

            var tokenRow = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (tokenRow is null) return Ok();

            if (tokenRow.RevokedAt is null)
            {
                tokenRow.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId)) return Unauthorized();

            var user = await _db.Users.AsNoTracking()
                .Where(u => u.UserId == userId && u.IsActive)
                .Select(u => new MeResponse(u.UserId, u.Email, u.FullName, u.Provider))
                .FirstOrDefaultAsync();

            if (user is null) return NotFound();
            return Ok(user);
        }

#if DEBUG
        // Helper để tạo bcrypt hash (dùng xong xóa cũng được)
        public record HashRequest(string Password);

        [HttpPost("dev/hash")]
        public IActionResult DevHash([FromBody] HashRequest req)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            return Ok(new { hash });
        }
#endif

        private string CreateJwt(User user)
        {
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var key = _config["Jwt:Key"]!;
            var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "30");

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("full_name", user.FullName),
        };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // 64 bytes -> 128 hex chars
        }



        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.IdToken))
                return BadRequest("IdToken is required.");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["GoogleAuth:ClientId"]! }
                });
            }
            catch
            {
                return Unauthorized("Invalid Google token.");
            }

            // payload.Email, payload.Name, payload.Subject(sub)
            var email = (payload.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("Google token has no email.");

            // Whitelist check
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user is null) return StatusCode(403, "Your email is not in the allowed list.");
            if (!user.IsActive) return StatusCode(403, "User is inactive.");

            // Ensure provider info stored
            user.Provider = "GOOGLE";
            user.ProviderUserId = payload.Subject; // sub
            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(payload.Name))
                user.FullName = payload.Name;

            // Issue tokens
            var accessToken = CreateJwt(user);

            var refreshToken = GenerateRefreshToken();
            var refreshHash = Sha256Hex(refreshToken);
            var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "14");

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = refreshHash,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
            });

            await _db.SaveChangesAsync();

            return Ok(new TokenResponse(accessToken, refreshToken));
        }
    }
}
