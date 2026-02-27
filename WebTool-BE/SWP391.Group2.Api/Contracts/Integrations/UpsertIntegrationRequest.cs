namespace SWP391.Group2.Api.Contracts.Integrations
{
    public record UpsertIntegrationRequest(
        string Provider,        // "JIRA" | "GITHUB"
        string? BaseUrl,
        string? ProjectKey,      // Jira key
        string? Org,             // GitHub org
        string? Token            // token plain (demo)
    );
}
