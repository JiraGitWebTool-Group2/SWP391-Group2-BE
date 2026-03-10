namespace SWP391.Group2.Api.Contracts.Sync
{
    public record StartSyncResponse(
        int SyncRunId,
        int ProjectId,
        string Status
    );
}
