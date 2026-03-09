using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public class RemoveLecturerFromClassHandler : IRequestHandler<RemoveLecturerFromClassCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveLecturerFromClassHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveLecturerFromClassCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await _context.Classes
            .FirstOrDefaultAsync(x => x.ClassId == request.ClassId, cancellationToken);

        if (classEntity == null)
            return false;

        if (!classEntity.LecturerUserId.HasValue)
            return false;

        if (classEntity.LecturerUserId.Value != request.LecturerId)
            return false;

        classEntity.LecturerUserId = null;
        classEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}