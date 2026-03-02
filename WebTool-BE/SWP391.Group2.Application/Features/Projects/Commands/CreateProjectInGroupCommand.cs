using MediatR;
using SWP391.Group2.Application.Features.Projects.Dtos;

namespace SWP391.Group2.Application.Features.Projects.Commands
{
    public record CreateProjectInGroupCommand(
        int GroupId,
        string ProjectName,
        string? JiraProjectKey,
        string? GithubOrg,
        string? Description
    ) : IRequest<ProjectDto>;
}