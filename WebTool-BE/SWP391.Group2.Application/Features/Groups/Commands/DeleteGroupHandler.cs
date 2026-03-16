using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;

public class DeleteGroupHandler : IRequestHandler<DeleteGroupCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteGroupHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.GroupId == request.GroupId, cancellationToken);

        if (group == null)
            return false;

        // 🔹 Remove all users in this group first
        var userGroups = await _context.UserGroups
            .Where(ug => ug.GroupId == request.GroupId)
            .ToListAsync(cancellationToken);

        _context.UserGroups.RemoveRange(userGroups);

        // 🔹 Remove group
        _context.Groups.Remove(group);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
