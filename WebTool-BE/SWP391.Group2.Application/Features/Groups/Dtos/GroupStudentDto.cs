namespace SWP391.Group2.Application.Features.Groups.Dtos;

public class GroupStudentDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SystemRole { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public string? GroupRole { get; set; }
}