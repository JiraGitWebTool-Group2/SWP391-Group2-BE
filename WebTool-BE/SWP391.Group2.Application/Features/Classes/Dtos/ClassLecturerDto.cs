namespace SWP391.Group2.Application.Features.Classes.Dtos;

public class ClassLecturerDto
{
    public int ClassId { get; set; }
    public int LecturerId { get; set; }
    public string LecturerEmail { get; set; } = null!;
    public string LecturerName { get; set; } = null!;
    public string LecturerRole { get; set; } = null!;
}