namespace SWP391.Group2.Api.Contracts.Snapshots
{
    public record SnapshotRepoSummaryDto(
        int RepoId,
        string RepoName,
        int CommitCount
    );
}
