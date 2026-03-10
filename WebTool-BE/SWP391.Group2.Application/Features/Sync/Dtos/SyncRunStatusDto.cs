using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Sync.Dtos
{
    public record SyncRunStatusDto(
        int SyncRunId,
        int ProjectId,
        string RunStatus,
        string? Notes,
        DateTime StartedAt,
        DateTime? FinishedAt,
        int? TriggeredByUserId,
        string? TriggeredByRole,
        int? SnapshotId
    );
}
