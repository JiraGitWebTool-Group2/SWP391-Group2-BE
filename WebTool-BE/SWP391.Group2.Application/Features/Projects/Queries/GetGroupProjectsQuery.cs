using MediatR;
using SWP391.Group2.Application.Features.Projects.Dtos;

namespace SWP391.Group2.Application.Features.Projects.Queries
{
    public record GetGroupProjectsQuery(int GroupId) : IRequest<List<ProjectDto>>;
}