namespace SWP391.Group2.Domain.Entities;

public class Class
{
    public int ClassId { get; set; }
    public int SemesterId { get; set; }
    public string ClassCode { get; set; } = null!;
    //public string CourseCode { get; set; } = null!;
    //public string? ClassName { get; set; }
    public int? LecturerUserId { get; set; }
    public string Status { get; set; } = "PLANNING";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Semester Semester { get; set; } = null!;
}