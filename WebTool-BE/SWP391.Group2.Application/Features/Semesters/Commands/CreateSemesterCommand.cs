using MediatR;
using SWP391.Group2.Application.Features.Semesters.Dtos;

namespace SWP391.Group2.Application.Features.Semesters.Commands;

public record CreateSemesterCommand(
    string Code,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status
) : IRequest<SemesterDto>;