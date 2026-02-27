namespace SWP391.Group2.Api.Contracts.Repositories
{
    public record UpdateRepositoryRequest(
        string RepoName,
        string? RepoUrl,
        string? DefaultBranch
    );
}
