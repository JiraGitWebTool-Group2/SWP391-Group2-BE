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

        // New field from updated DB
        public string? TriggeredByRole { get; set; }   // ADMIN / LECTURER / STUDENT

        public string TriggerType { get; set; } = default!; // MANUAL / AUTO
        public string ScopeType { get; set; } = default!;   // SPRINT / BACKLOG / CUSTOM
        public int? SprintId { get; set; }

        public bool IncludeJira { get; set; }
        public bool IncludeGithub { get; set; }

        // RUNNING / SUCCESS / FAILED / PARTIAL
        public string RunStatus { get; set; } = "RUNNING";

        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Project Project { get; set; } = default!;
        public User? TriggeredByUser { get; set; }
        public ICollection<Snapshot> Snapshots { get; set; } = new List<Snapshot>();
    }

}
