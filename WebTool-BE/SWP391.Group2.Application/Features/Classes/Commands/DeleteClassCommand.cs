using MediatR;

namespace SWP391.Group2.Application.Features.Classes.Commands;

public record DeleteClassCommand(int ClassId) : IRequest<bool>;