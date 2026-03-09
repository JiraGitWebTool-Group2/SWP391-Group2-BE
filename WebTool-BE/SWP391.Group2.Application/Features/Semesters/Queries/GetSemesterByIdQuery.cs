using MediatR;
using SWP391.Group2.Application.Features.Semesters.Dtos;

namespace SWP391.Group2.Application.Features.Semesters.Queries;

public record GetSemesterByIdQuery(int SemesterId) : IRequest<SemesterDto?>;