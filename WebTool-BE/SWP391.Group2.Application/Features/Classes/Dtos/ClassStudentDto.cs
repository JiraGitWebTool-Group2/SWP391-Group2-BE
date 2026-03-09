namespace SWP391.Group2.Application.Features.Classes.Dtos;

public class ClassStudentDto
{
    public int ClassId { get; set; }
    public int StudentId { get; set; }
    public string StudentEmail { get; set; } = null!;
    public string StudentName { get; set; } = null!;
    public string StudentRole { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}