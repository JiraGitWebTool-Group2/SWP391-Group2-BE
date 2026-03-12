using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class SnapshotPullRequest
    {
        public int SnapshotPullRequestId { get; set; }   // PK
        public int SnapshotId { get; set; }              // FK -> Snapshots(snapshot_id)
        public int PullRequestId { get; set; }           // FK -> GitHubPullRequests(pull_request_id)

        // Navigation
        public Snapshot Snapshot { get; set; } = default!;
        public GitHubPullRequest PullRequest { get; set; } = default!;
    }
}
