namespace SWP391.Group2.Api.Contracts.Projects
{
    public record ProjectDto(
        int ProjectId,
        int GroupId,
        string ProjectName,
        string? JiraProjectKey,
        string? GithubOrg,
        string? Description,
        DateTime CreatedAt
    );
}