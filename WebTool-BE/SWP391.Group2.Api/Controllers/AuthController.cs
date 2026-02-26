using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWP391.Group2.Application.Features.Auth;
using SWP391.Group2.Domain.Entities;
using SWP391.Group2.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public record GoogleLoginRequest(string IdToken);

        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] GoogleLoginRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _mediator.Send(new GoogleLoginCommand(req.IdToken), ct);
                return Ok(new { accessToken = result.AccessToken, refreshToken = result.RefreshToken });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
