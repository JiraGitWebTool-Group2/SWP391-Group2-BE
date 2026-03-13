using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public class CreateClassHandler : IRequestHandler<CreateClassCommand, ClassDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClassHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassDto> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        var status = request.Status.Trim().ToUpperInvariant();

        if (status != "PLANNING" && status != "ACTIVE" && status != "ARCHIVED")
            throw new ArgumentException("Status must be PLANNING, ACTIVE or ARCHIVED.");

        var semester = await _context.Semesters
            .FirstOrDefaultAsync(x => x.SemesterId == request.SemesterId, cancellationToken);

        if (semester == null)
            throw new ArgumentException($"Semester with id {request.SemesterId} was not found.");

        if (request.LecturerUserId.HasValue)
        {
            var lecturerExists = await _context.Users
                .AnyAsync(x => x.UserId == request.LecturerUserId.Value, cancellationToken);

            if (!lecturerExists)
                throw new ArgumentException($"Lecturer with id {request.LecturerUserId.Value} was not found.");
        }

        var duplicated = await _context.Classes.AnyAsync(
            x => x.SemesterId == request.SemesterId
              //&& x.CourseCode == request.CourseCode
              && x.ClassCode == request.ClassCode,
            cancellationToken);

        if (duplicated)
            throw new ArgumentException("Class already exists in this semester with the same course code and class code.");

        var entity = new Class
        {
            SemesterId = request.SemesterId,
            ClassCode = request.ClassCode.Trim(),
            //CourseCode = request.CourseCode.Trim(),
            //ClassName = string.IsNullOrWhiteSpace(request.ClassName) ? null : request.ClassName.Trim(),
            LecturerUserId = request.LecturerUserId,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Classes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClassDto
        {
            ClassId = entity.ClassId,
            SemesterId = entity.SemesterId,
            SemesterCode = semester.Code,
            //SemesterName = semester.Name,
            ClassCode = entity.ClassCode,
            //CourseCode = entity.CourseCode,
            //ClassName = entity.ClassName,
            LecturerUserId = entity.LecturerUserId,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}