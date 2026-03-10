using MediatR;

namespace SWP391.Group2.Application.Features.Lecturers.Queries;

public record GetLecturerDetailQuery(int LecturerId) : IRequest<LecturerDetailDto>;

public class LecturerDetailDto
{
    public int LecturerId { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
}