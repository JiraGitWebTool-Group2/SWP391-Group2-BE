using MediatR;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public record RemoveLecturerFromClassCommand(
    int ClassId,
    int LecturerId
) : IRequest<bool>;