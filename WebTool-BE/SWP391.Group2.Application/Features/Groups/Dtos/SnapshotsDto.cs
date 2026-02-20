
namespace SWP391.Group2.Application.Features.Groups.Dtos
{
    public record SyncRunBriefDto(
         int SyncRunId,
         int ProjectId,
         string ProjectName,
         string TriggerType,
         string ScopeType,
         int? SprintId,
         string RunStatus,
         DateTime StartedAt,
         DateTime? FinishedAt
    );

    public record SnapshotListItemDto(
        int SnapshotId,
        DateTime CapturedAt,
        string? Label,
        SyncRunBriefDto SyncRun
    );
}
