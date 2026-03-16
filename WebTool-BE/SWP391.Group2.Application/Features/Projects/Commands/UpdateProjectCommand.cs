using MediatR;

namespace SWP391.Group2.Application.Features.Projects.Commands
{
    public record UpdateProjectCommand(
        int ProjectId,
        string ProjectCode,
        string ProjectName,
        string? Description,
        string? Requirement
    ) : IRequest<bool>;
}