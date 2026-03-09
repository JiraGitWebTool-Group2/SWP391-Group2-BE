namespace SWP391.Group2.Domain.Entities;

public class ClassStudent
{
    public int ClassStudentId { get; set; }
    public int ClassId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }

    public Class Class { get; set; } = null!;
    public User User { get; set; } = null!;
}