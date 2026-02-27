using MediatR;
using SWP391.Group2.Application.Features.Users.Dtos;


namespace SWP391.Group2.Application.Features.Users.Command
{
    public record AddUserCommand(CreateUserRequestDto Request) : IRequest<UserDto>;
}
