using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Api.Contracts.Auth;
using SWP391.Group2.Application.Features.Auth.Command;
using SWP391.Group2.Application.Features.Auth.Queries;
using System.Security.Claims;


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

        // POST /api/auth/google
        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] GoogleLoginRequest req, CancellationToken ct)
        {
            try
            {
                var pair = await _mediator.Send(new GoogleLoginCommand(req.IdToken, req.Role), ct);
                return Ok(new TokenResponse(pair.AccessToken, pair.RefreshToken));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        {
            try
            {
                var pair = await _mediator.Send(new RefreshTokenCommand(req.RefreshToken), ct);
                return Ok(new TokenResponse(pair.AccessToken, pair.RefreshToken));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest req, CancellationToken ct)
        {
            try
            {
                await _mediator.Send(new LogoutCommand(req.RefreshToken), ct);
                return Ok(new { message = "Logged out" });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        // GET /api/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId))
                return Unauthorized("Invalid token.");

            try
            {
                var me = await _mediator.Send(new GetMeQuery(userId), ct);
                return Ok(me);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

#if DEBUG
        // POST /api/auth/dev/google   (DEV-ONLY)
        [HttpPost("dev/google")]
        public async Task<IActionResult> DevGoogle([FromBody] DevLoginRequest req, CancellationToken ct)
        {
            try
            {
                var pair = await _mediator.Send(new DevLoginCommand(req.Email), ct);
                return Ok(new TokenResponse(pair.AccessToken, pair.RefreshToken));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        }
#endif
    }
}
