namespace SWP391.Group2.Api.Contracts.Projects;

public record ConfigureIntegrationRequest(
    string JiraProjectKey,
    string GithubOrg
);