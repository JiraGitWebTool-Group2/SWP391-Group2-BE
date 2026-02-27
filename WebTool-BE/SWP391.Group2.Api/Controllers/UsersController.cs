using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.Users.Command;
using SWP391.Group2.Application.Features.Users.Dtos;

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
    }
}
