using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.Users.Command;
using SWP391.Group2.Application.Features.Users.Dtos;
using SWP391.Group2.Application.Features.Users.Queries;
using System.Security.Claims;

namespace SWP391.Group2.Api.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public sealed class ImportUsersExcelRequest
        {
            public IFormFile File { get; set; } = default!;
        }

        // POST /api/users
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] CreateUserRequestDto request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var dto = await _mediator.Send(new AddUserCommand(request));
                return CreatedAtAction(nameof(GetById), new { id = dto.UserId }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // stub để CreatedAtAction không bị đỏ (bạn có GET thật rồi thì thay)
        [HttpGet("{id:int}")]
        public IActionResult GetById([FromRoute] int id) => Ok();

        // POST /api/users/import-excel
        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportUsersExcel([FromForm] ImportUsersExcelRequest request, CancellationToken ct)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { message = "File rỗng." });

            byte[] content;
            using (var ms = new MemoryStream())
            {
                await request.File.CopyToAsync(ms, ct);
                content = ms.ToArray();
            }

            var result = await _mediator.Send(
                new ImportUsersFromExcelCommand(content, request.File.FileName),
                ct);

            return Ok(result);
        }

        // GET /api/users/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken ct)
        {
            // Lấy email từ claims (tùy hệ auth, claim key có thể là "email" hoặc ClaimTypes.Email)
            var email =
                User.FindFirstValue(ClaimTypes.Email) ??
                User.FindFirstValue("email") ??
                User.FindFirstValue(ClaimTypes.Name); // fallback (tùy bạn set claim)

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new { message = "Token không chứa email claim." });

            try
            {
                var dto = await _mediator.Send(new GetMeQuery(email), ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }


}
