using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;


namespace SWP391.Group2.Application.Features.Groups.Commands
{
    public class RemoveStudentFromGroupCommandHandler
        : IRequestHandler<RemoveStudentFromGroupCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public RemoveStudentFromGroupCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            RemoveStudentFromGroupCommand request,
            CancellationToken cancellationToken)
        {
            var student = await _context.UserGroups

                .FirstOrDefaultAsync(
                    x => x.GroupId == request.GroupId &&
                         x.UserId == request.UserId,
                    cancellationToken);

            if (student == null)
                return false;

            _context.UserGroups.Remove(student);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
