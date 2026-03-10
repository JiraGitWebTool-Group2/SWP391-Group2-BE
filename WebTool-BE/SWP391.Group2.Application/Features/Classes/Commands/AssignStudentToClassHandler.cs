using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public class AssignStudentToClassHandler : IRequestHandler<AssignStudentToClassCommand, ClassStudentDto>
{
    private readonly IApplicationDbContext _context;

    public AssignStudentToClassHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassStudentDto> Handle(AssignStudentToClassCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await _context.Classes
            .FirstOrDefaultAsync(x => x.ClassId == request.ClassId, cancellationToken);

        if (classEntity == null)
            throw new ArgumentException($"Class with id {request.ClassId} was not found.");

        var student = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == request.StudentId, cancellationToken);

        if (student == null)
            throw new ArgumentException($"Student with id {request.StudentId} was not found.");

        if (!string.Equals(student.System_Role, "STUDENT", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"User with id {request.StudentId} is not a student.");

        var existing = await _context.ClassStudents
            .FirstOrDefaultAsync(
                x => x.ClassId == request.ClassId && x.UserId == request.StudentId,
                cancellationToken);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.JoinedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }

            // lấy group của student
            var group = await (
                from ug in _context.UserGroups
                join g in _context.Groups on ug.GroupId equals g.GroupId
                where ug.UserId == student.UserId && g.ClassId == request.ClassId
                select new { g.GroupId, g.GroupName }
            ).FirstOrDefaultAsync(cancellationToken);

            return new ClassStudentDto
            {
                ClassId = existing.ClassId,
                StudentId = student.UserId,
                StudentEmail = student.Email,
                StudentName = student.FullName,
                StudentRole = student.System_Role,
                JoinedAt = existing.JoinedAt,
                IsActive = existing.IsActive,
                GroupId = group?.GroupId,
                GroupName = group?.GroupName
            };
        }

        var entity = new ClassStudent
        {
            ClassId = request.ClassId,
            UserId = request.StudentId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ClassStudents.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // lấy group nếu student đã có
        var studentGroup = await (
            from ug in _context.UserGroups
            join g in _context.Groups on ug.GroupId equals g.GroupId
            where ug.UserId == student.UserId && g.ClassId == request.ClassId
            select new { g.GroupId, g.GroupName }
        ).FirstOrDefaultAsync(cancellationToken);

        return new ClassStudentDto
        {
            ClassId = entity.ClassId,
            StudentId = student.UserId,
            StudentEmail = student.Email,
            StudentName = student.FullName,
            StudentRole = student.System_Role,
            JoinedAt = entity.JoinedAt,
            IsActive = entity.IsActive,
            GroupId = studentGroup?.GroupId,
            GroupName = studentGroup?.GroupName
        };
    }
}