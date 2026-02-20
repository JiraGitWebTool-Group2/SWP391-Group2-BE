using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.SyncRun.Dtos
{
    public class TriggerSyncDto
    {
        public string TriggerType { get; set; } = "MANUAL";   // MANUAL/AUTO
        public string ScopeType { get; set; } = "SPRINT";     // SPRINT/BACKLOG/CUSTOM
        public int? SprintId { get; set; }

        public bool IncludeJira { get; set; } = true;
        public bool IncludeGithub { get; set; } = true;
    }
}
