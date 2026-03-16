using MediatR;

namespace SWP391.Group2.Application.Features.Groups.Commands
{
    public record RemoveStudentFromGroupCommand(int GroupId, int UserId) : IRequest<bool>;
}
