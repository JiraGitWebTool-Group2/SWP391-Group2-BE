namespace SWP391.Group2.Api.Contracts.Lecturers;

public class LecturerDto
{
    public int LecturerId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string SystemRole { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}   