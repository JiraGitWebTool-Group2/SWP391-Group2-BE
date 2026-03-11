namespace SWP391.Group2.Api.Contracts.Groups
{
    public record AddStudentToGroupRequest(
        int UserId,   // User ID of the student to be added
        int RoleId    // Role ID (e.g., 'LECTURER', 'TEAM_LEADER', 'TEAM_MEMBER')
    );
}
