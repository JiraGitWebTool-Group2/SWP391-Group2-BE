namespace SWP391.Group2.Api.Contracts.Repositories
{
    public record CreateRepositoryRequest(
        string RepoName,          // ví dụ: "swp391-repo-01"
        string? RepoUrl,          // ví dụ: "https://github.com/org/swp391-repo-01"
        string? DefaultBranch     // ví dụ: "main"
    );
}
