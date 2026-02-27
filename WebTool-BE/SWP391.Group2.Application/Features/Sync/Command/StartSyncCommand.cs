using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Sync.Command
{
    public record StartSyncCommand(
        int ProjectId,
        bool IncludeJira,
        bool IncludeGithub,
        string ScopeType,
        int? SprintId,
        int? TriggeredByUserId, // lấy từ JWT, AUTO thì null
        string TriggerType      // "MANUAL" | "AUTO"
    ) : IRequest<int>;          // return SyncRunId
}
