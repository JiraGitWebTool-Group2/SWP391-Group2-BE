namespace SWP391.Group2.Application.Features.Semesters.Dtos;

public class SemesterDto
{
    public int SemesterId { get; set; }
    public string Code { get; set; } = null!;
    //public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}