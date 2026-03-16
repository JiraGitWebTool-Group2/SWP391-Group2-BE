using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Projects.Commands
{
    public class DeleteProjectCommandHandler
        : IRequestHandler<DeleteProjectCommand, bool>
    {
        private readonly IApplicationDbContext _db;

        public DeleteProjectCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Handle(
            DeleteProjectCommand request,
            CancellationToken cancellationToken)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(
                    x => x.ProjectId == request.ProjectId,
                    cancellationToken);

            if (project == null)
                throw new KeyNotFoundException("Project not found");

            _db.Projects.Remove(project);

            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}