using MediatR;
using SWP391.Group2.Application.Features.Classes.Dtos;

namespace SWP391.Group2.Application.Features.Classes.Queries;

public record GetClassByIdQuery(int ClassId) : IRequest<ClassDto?>;