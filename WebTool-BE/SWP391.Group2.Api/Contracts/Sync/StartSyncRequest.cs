namespace SWP391.Group2.Api.Contracts.Sync
{
    public record StartSyncRequest(
        int ProjectId,
        bool IncludeJira,
        bool IncludeGithub,
        string ScopeType,   // "SPRINT" | "BACKLOG" | "CUSTOM"
        int? SprintId,      // chỉ dùng khi ScopeType = "SPRINT"

        DateTime? GithubFrom,
        DateTime? GithubTo,
        bool SyncGithubCommits = true,
        bool SyncGithubPullRequests = false,
        string? GithubSyncMode = null // FULL | INCREMENTAL | CUSTOM_RANGE | SINGLE_DAY
    );
}
