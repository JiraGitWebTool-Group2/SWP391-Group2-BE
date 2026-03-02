using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Projects.Queries
{
    public class GetProjectByIdHandler
        : IRequestHandler<GetProjectByIdQuery, Project>
    {
        private readonly IApplicationDbContext _context;

        public GetProjectByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Project> Handle(
            GetProjectByIdQuery request,
            CancellationToken cancellationToken)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.ProjectId == request.ProjectId,
                    cancellationToken);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            return project;
        }
    }
}