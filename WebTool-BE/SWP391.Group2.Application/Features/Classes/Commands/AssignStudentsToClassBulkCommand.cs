using MediatR;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public record AssignStudentsToClassBulkCommand(
    int ClassId,
    List<int> StudentIds
) : IRequest<List<ClassStudentDto>>;