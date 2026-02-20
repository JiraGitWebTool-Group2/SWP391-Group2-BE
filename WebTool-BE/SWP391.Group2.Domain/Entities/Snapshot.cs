using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class Snapshot
    {
        public int SnapshotId { get; set; }
        public int SyncRunId { get; set; }
        public DateTime CapturedAt { get; set; }
        public string? Label { get; set; }

        // Navigation
        public SyncRun SyncRun { get; set; } = default!;
        public ICollection<IssueCommitLink> IssueCommitLinks { get; set; } = new List<IssueCommitLink>();
    }
}
