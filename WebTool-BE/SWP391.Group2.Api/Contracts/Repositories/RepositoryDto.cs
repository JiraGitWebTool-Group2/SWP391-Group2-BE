namespace SWP391.Group2.Api.Contracts.Repositories
{
    public record RepositoryDto(
        int RepoId,
        int ProjectId,
        string RepoName,
        string? RepoUrl,
        string? DefaultBranch,
        DateTime CreatedAt
    );
}
