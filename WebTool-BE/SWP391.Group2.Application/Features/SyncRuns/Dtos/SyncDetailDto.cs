using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.SyncRun.Dtos
{
    public class SyncDetailDto
    {
        public int SyncRunId { get; set; }
        public int ProjectId { get; set; }

        public string TriggerType { get; set; } = default!;
        public string ScopeType { get; set; } = default!;
        public string RunStatus { get; set; } = default!;

        public bool IncludeJira { get; set; }
        public bool IncludeGithub { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public int SnapshotCount { get; set; }
    }
}
