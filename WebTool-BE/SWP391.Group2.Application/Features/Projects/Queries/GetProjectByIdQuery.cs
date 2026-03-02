using MediatR;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Projects.Queries
{
    public class GetProjectByIdQuery : IRequest<Project>
    {
        public int ProjectId { get; }

        public GetProjectByIdQuery(int projectId)
        {
            ProjectId = projectId;
        }
    }
}