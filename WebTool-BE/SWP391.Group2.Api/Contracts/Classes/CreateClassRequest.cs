namespace SWP391.Group2.Api.Contracts.Classes;

public class CreateClassRequest
{
    public int SemesterId { get; set; }
    public string ClassCode { get; set; } = null!;
    public string CourseCode { get; set; } = null!;
    public string? ClassName { get; set; }
    public int? LecturerUserId { get; set; }
    public string Status { get; set; } = "PLANNING";
}