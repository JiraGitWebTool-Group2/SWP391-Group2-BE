using MediatR;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public record UpdateClassCommand(
    int ClassId,
    int SemesterId,
    string ClassCode,
    //string CourseCode,
    //string? ClassName,
    int? LecturerUserId,
    string Status
) : IRequest<ClassDto?>;