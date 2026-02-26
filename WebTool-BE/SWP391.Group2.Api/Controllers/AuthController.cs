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

        public record TokenResponse(string AccessToken, string RefreshToken);

        // ====== 1) REAL GOOGLE LOGIN (sẽ test sau khi có idToken) ======
        public record GoogleLoginRequest(string IdToken);

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.IdToken))
                return BadRequest("IdToken is required.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _config["GoogleAuth:ClientId"]! }
                    });
            }
            catch
            {
                return Unauthorized("Invalid Google token.");
            }

            var email = (payload.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("Google token has no email.");

            return await IssueTokensForWhitelistedEmail(email);
        }

        // ====== 2) DEV-ONLY: giả lập google login để test Swagger không cần FE ======
#if DEBUG
        public record DevGoogleRequest(string Email);

        [HttpPost("dev/google")]
        public async Task<IActionResult> DevGoogle([FromBody] DevGoogleRequest req)
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            return await IssueTokensForWhitelistedEmail(email);
        }
#endif

        // ====== 3) ME ======
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId)) return Unauthorized();

            var user = await _db.Users.AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(u => new { u.UserId, u.Email, u.FullName, u.Provider, u.IsActive })
                .FirstOrDefaultAsync();

            if (user is null) return NotFound();
            return Ok(user);
        }

        // ====== helper: whitelist + phát token + lưu refresh ======
        private async Task<IActionResult> IssueTokensForWhitelistedEmail(string emailLower)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);
            if (user is null) return StatusCode(403, "Email not in allowed list.");
            if (!user.IsActive) return StatusCode(403, "User inactive.");

            // Provider set cho đẹp dữ liệu (optional)
            user.Provider ??= "GOOGLE";

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
            return Convert.ToHexString(bytes);
        }
    }
}
