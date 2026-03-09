using MediatR;

namespace SWP391.Group2.Application.Features.Semesters.Commands;

public record DeleteSemesterCommand(int SemesterId) : IRequest<bool>;