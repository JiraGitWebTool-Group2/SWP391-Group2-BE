using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Queries;

public class GetClassByIdHandler : IRequestHandler<GetClassByIdQuery, ClassDto?>
{
    private readonly IApplicationDbContext _context;

    public GetClassByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassDto?> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.ClassId == request.ClassId)
            .Join(
                _context.Semesters.AsNoTracking(),
                c => c.SemesterId,
                s => s.SemesterId,
                (c, s) => new ClassDto
                {
                    ClassId = c.ClassId,
                    SemesterId = c.SemesterId,
                    SemesterCode = s.Code,
                    SemesterName = s.Name,
                    ClassCode = c.ClassCode,
                    CourseCode = c.CourseCode,
                    ClassName = c.ClassName,
                    LecturerUserId = c.LecturerUserId,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
            .FirstOrDefaultAsync(cancellationToken);
    }
}