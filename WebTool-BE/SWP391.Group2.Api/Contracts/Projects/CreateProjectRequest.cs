namespace SWP391.Group2.Api.Contracts.Projects
{
    namespace SWP391.Group2.Api.Contracts.Projects
    {
        public record CreateProjectRequest(
            string ProjectName,
            string? JiraProjectKey,
            string? GithubOrg,
            string? Description
        );
    }
}
