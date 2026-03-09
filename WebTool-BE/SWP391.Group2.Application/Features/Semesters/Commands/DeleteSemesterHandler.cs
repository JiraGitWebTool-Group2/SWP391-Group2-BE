using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

namespace SWP391.Group2.Application.Features.Semesters.Commands;

public class DeleteSemesterHandler : IRequestHandler<DeleteSemesterCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSemesterHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSemesterCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Semesters
            .FirstOrDefaultAsync(x => x.SemesterId == request.SemesterId, cancellationToken);

        if (entity == null)
            return false;

        _context.Semesters.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}