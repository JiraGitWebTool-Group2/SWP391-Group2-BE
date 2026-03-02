namespace SWP391.Group2.Api.Contracts.Integrations
{
    // Dùng cho POST (create mới). Nếu đã tồn tại thì API sẽ trả 409.
    public record CreateIntegrationRequest(
        string Provider,        // "JIRA" | "GITHUB"
        string? BaseUrl,
        string? ProjectKey,      // Jira key
        string? Org,             // GitHub org
        string? Token            // token plain (demo)
    );
}