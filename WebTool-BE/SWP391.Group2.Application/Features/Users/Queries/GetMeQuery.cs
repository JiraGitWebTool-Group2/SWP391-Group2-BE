using MediatR;
using SWP391.Group2.Application.Features.Users.Dtos;

namespace SWP391.Group2.Application.Features.Users.Queries
{
    public record GetMeQuery(string Email) : IRequest<UserDto>;
}