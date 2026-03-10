using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public class AssignStudentsToClassBulkHandler : IRequestHandler<AssignStudentsToClassBulkCommand, List<ClassStudentDto>>
{
    private readonly IApplicationDbContext _context;

    public AssignStudentsToClassBulkHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClassStudentDto>> Handle(AssignStudentsToClassBulkCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await _context.Classes
            .FirstOrDefaultAsync(x => x.ClassId == request.ClassId, cancellationToken);

        if (classEntity == null)
            throw new ArgumentException($"Class with id {request.ClassId} was not found.");

        var distinctStudentIds = request.StudentIds
            .Distinct()
            .ToList();

        if (distinctStudentIds.Count == 0)
            return new List<ClassStudentDto>();

        var students = await _context.Users
            .Where(x => distinctStudentIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        var foundIds = students.Select(x => x.UserId).ToHashSet();
        var missingIds = distinctStudentIds.Where(x => !foundIds.Contains(x)).ToList();

        if (missingIds.Count > 0)
            throw new ArgumentException($"Students not found: {string.Join(", ", missingIds)}");

        var invalidRoleIds = students
            .Where(x => !string.Equals(x.System_Role, "STUDENT", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.UserId)
            .ToList();

        if (invalidRoleIds.Count > 0)
            throw new ArgumentException($"Users are not students: {string.Join(", ", invalidRoleIds)}");

        var existingMemberships = await _context.ClassStudents
            .Where(x => x.ClassId == request.ClassId && distinctStudentIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        var existingMap = existingMemberships.ToDictionary(x => x.UserId, x => x);

        foreach (var studentId in distinctStudentIds)
        {
            if (existingMap.TryGetValue(studentId, out var existing))
            {
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    existing.JoinedAt = DateTime.UtcNow;
                }

                continue;
            }

            _context.ClassStudents.Add(new ClassStudent
            {
                ClassId = request.ClassId,
                UserId = studentId,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.ClassStudents
            .AsNoTracking()
            .Where(x => x.ClassId == request.ClassId && distinctStudentIds.Contains(x.UserId))
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
            .OrderBy(x => x.StudentId)
            .ToListAsync(cancellationToken);

        return result;
    }
}