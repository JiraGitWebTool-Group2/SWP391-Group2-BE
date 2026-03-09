using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public class RemoveStudentFromClassHandler : IRequestHandler<RemoveStudentFromClassCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveStudentFromClassHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public RemoveStudentFromClassHandler(IApplicationDbContext context, bool _ = true)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveStudentFromClassCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ClassStudents
            .FirstOrDefaultAsync(
                x => x.ClassId == request.ClassId && x.UserId == request.StudentId,
                cancellationToken);

        if (entity == null)
            return false;

        _context.ClassStudents.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}