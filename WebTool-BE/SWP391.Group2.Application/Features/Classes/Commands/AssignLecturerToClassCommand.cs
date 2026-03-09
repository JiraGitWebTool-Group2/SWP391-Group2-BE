using MediatR;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public record AssignLecturerToClassCommand(
    int ClassId,
    int LecturerId
) : IRequest<ClassLecturerDto>;