using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Projects.Commands
{
    public class UpdateProjectCommandHandler
        : IRequestHandler<UpdateProjectCommand, bool>
    {
        private readonly IApplicationDbContext _db;

        public UpdateProjectCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Handle(
            UpdateProjectCommand request,
            CancellationToken cancellationToken)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(
                    x => x.ProjectId == request.ProjectId,
                    cancellationToken);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            //project.ProjectCode = request.ProjectCode;
            project.ProjectName = request.ProjectName;
            project.Description = request.Description;
            project.Requirement = request.Requirement;

            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}