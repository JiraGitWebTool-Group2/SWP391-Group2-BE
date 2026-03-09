namespace SWP391.Group2.Application.Features.Classes.Dtos;

public class ClassDto
{
    public int ClassId { get; set; }
    public int SemesterId { get; set; }
    public string SemesterCode { get; set; } = null!;
    public string SemesterName { get; set; } = null!;
    public string ClassCode { get; set; } = null!;
    public string CourseCode { get; set; } = null!;
    public string? ClassName { get; set; }
    public int? LecturerUserId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}