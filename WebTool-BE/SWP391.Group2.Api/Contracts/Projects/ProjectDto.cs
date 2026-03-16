namespace SWP391.Group2.Api.Contracts.Projects
{
    public record ProjectDto(
        int ProjectId,
        int GroupId,
        string ProjectCode,
        string ProjectName,
        string? Requirement,
        string? Description,
        DateTime CreatedAt
    );
}