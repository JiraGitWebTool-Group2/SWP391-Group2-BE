using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class IssueCommitLink
    {
        public int LinkId { get; set; }
        public int SnapshotId { get; set; }
        public int IssueId { get; set; }
        public int CommitId { get; set; }

        // Navigation
        public Snapshot Snapshot { get; set; } = default!;
        public JiraIssue JiraIssue { get; set; } = default!;
        public GitHubCommit GitHubCommit { get; set; } = default!;
    }
}
