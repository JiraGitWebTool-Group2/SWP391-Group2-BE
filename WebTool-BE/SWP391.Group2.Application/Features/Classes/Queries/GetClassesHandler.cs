using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Queries;

public class GetClassesHandler : IRequestHandler<GetClassesQuery, List<ClassDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClassesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClassDto>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Classes
            .AsNoTracking()
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
                });

        if (request.SemesterId.HasValue)
        {
            query = query.Where(x => x.SemesterId == request.SemesterId.Value);
        }

        return await query
            .OrderByDescending(x => x.ClassId)
            .ToListAsync(cancellationToken);
    }
}