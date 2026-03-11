namespace SWP391.Group2.Api.Contracts.Groups
{
    public record CreateGroupRequest(
        string GroupName,
        string? Description,
        int ClassId
    );
}