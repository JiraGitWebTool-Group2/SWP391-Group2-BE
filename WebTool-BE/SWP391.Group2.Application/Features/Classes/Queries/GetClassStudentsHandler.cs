using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Queries;

public class GetClassStudentsHandler : IRequestHandler<GetClassStudentsQuery, List<ClassStudentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClassStudentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClassStudentDto>> Handle(GetClassStudentsQuery request, CancellationToken cancellationToken)
    {
        var classExists = await _context.Classes
            .AnyAsync(x => x.ClassId == request.ClassId, cancellationToken);

        if (!classExists)
            throw new ArgumentException($"Class with id {request.ClassId} was not found.");

        return await _context.ClassStudents
            .AsNoTracking()
            .Where(x => x.ClassId == request.ClassId && x.IsActive)
            .Join(
                _context.Users.AsNoTracking(),
                cs => cs.UserId,
                u => u.UserId,
                (cs, u) => new ClassStudentDto
                {
                    ClassId = cs.ClassId,
                    StudentId = u.UserId,
                    StudentEmail = u.Email,
                    StudentName = u.FullName,
                    StudentRole = u.System_Role,
                    JoinedAt = cs.JoinedAt,
                    IsActive = cs.IsActive
                })
            .OrderBy(x => x.StudentName)
            .ToListAsync(cancellationToken);
    }
}