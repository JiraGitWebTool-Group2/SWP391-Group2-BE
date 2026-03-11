using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public class AssignLecturerToClassHandler : IRequestHandler<AssignLecturerToClassCommand, ClassLecturerDto>
{
    private readonly IApplicationDbContext _context;

    public AssignLecturerToClassHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassLecturerDto> Handle(AssignLecturerToClassCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await _context.Classes
            .FirstOrDefaultAsync(x => x.ClassId == request.ClassId, cancellationToken);

        if (classEntity == null)
            throw new ArgumentException($"Class with id {request.ClassId} was not found.");

        var lecturer = await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == request.LecturerId, cancellationToken);

        if (lecturer == null)
            throw new ArgumentException($"Lecturer with id {request.LecturerId} was not found.");

        if (!string.Equals(lecturer.SystemRole, "LECTURER", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"User with id {request.LecturerId} is not a lecturer.");

        classEntity.LecturerUserId = lecturer.UserId;
        classEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ClassLecturerDto
        {
            ClassId = classEntity.ClassId,
            LecturerId = lecturer.UserId,
            LecturerEmail = lecturer.Email,
            LecturerName = lecturer.FullName,
            LecturerRole = lecturer.SystemRole
        };
    }
}