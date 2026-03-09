using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public class DeleteClassHandler : IRequestHandler<DeleteClassCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteClassHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Classes
            .FirstOrDefaultAsync(x => x.ClassId == request.ClassId, cancellationToken);

        if (entity == null)
            return false;

        _context.Classes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}