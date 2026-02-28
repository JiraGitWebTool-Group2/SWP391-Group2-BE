namespace SWP391.Group2.Api.Contracts.Snapshots
{
    public record SnapshotCommitItemDto(
        int CommitId,
        string CommitHash,
        string Message,
        DateTime CommittedAt,
        string? CommitUrl,
        int RepoId,
        string RepoName
    );
}
