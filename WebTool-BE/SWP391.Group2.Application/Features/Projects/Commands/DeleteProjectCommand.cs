using MediatR;

namespace SWP391.Group2.Application.Features.Projects.Commands
{
    public record DeleteProjectCommand(int ProjectId) : IRequest<bool>;
}