using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Queries;

public class GetClassLecturerHandler : IRequestHandler<GetClassLecturerQuery, ClassLecturerDto?>
{
    private readonly IApplicationDbContext _context;

    public GetClassLecturerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassLecturerDto?> Handle(GetClassLecturerQuery request, CancellationToken cancellationToken)
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.ClassId == request.ClassId && c.LecturerUserId != null)
            .Join(
                _context.Users.AsNoTracking(),
                c => c.LecturerUserId,
                u => u.UserId,
                (c, u) => new ClassLecturerDto
                {
                    ClassId = c.ClassId,
                    LecturerId = u.UserId,
                    LecturerEmail = u.Email,
                    LecturerName = u.FullName,
                    LecturerRole = u.SystemRole
                })
            .FirstOrDefaultAsync(cancellationToken);
    }
}