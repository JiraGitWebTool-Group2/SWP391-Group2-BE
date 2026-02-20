using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class SyncRun
    {
        public int SyncRunId { get; set; }
        public int ProjectId { get; set; }
        public int? TriggeredByUserId { get; set; }
        public string TriggerType { get; set; } = default!; // MANUAL/AUTO
        public string ScopeType { get; set; } = default!;   // SPRINT/BACKLOG/CUSTOM
        public int? SprintId { get; set; }
        public bool IncludeJira { get; set; }
        public bool IncludeGithub { get; set; }
        public string RunStatus { get; set; } = default!;   // RUNNING/SUCCESS/FAILED
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Project Project { get; set; } = default!;
        public ICollection<Snapshot> Snapshots { get; set; } = new List<Snapshot>();
    }

}
