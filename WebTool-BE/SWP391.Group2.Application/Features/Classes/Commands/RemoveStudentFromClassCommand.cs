using MediatR;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public record RemoveStudentFromClassCommand(
    int ClassId,
    int StudentId
) : IRequest<bool>;