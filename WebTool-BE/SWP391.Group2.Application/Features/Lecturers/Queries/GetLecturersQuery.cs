using MediatR;
using SWP391.Group2.Application.Features.Lecturers.Dtos;

namespace SWP391.Group2.Application.Features.Lecturers.Queries;

public record GetLecturersQuery() : IRequest<List<LecturerDto>>;