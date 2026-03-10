using MediatR;

namespace SWP391.Group2.Application.Features.Lecturers.Queries;

public record GetLecturerClassesQuery(int LecturerId) : IRequest<List<LecturerClassDto>>;

public class LecturerClassDto
{
    public int ClassId { get; set; }
    public string ClassCode { get; set; } = default!;
}