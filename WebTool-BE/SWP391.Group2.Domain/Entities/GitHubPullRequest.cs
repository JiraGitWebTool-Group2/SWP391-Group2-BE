using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class GitHubPullRequest
    {
        public int PullRequestId { get; set; }   // PK
        public int RepoId { get; set; }          // FK -> Repositories(repo_id)

        public int PrNumber { get; set; }        // Unique per repo
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string State { get; set; } = string.Empty;   // OPEN / CLOSED / MERGED

        public string? AuthorLogin { get; set; }

        public DateTime CreatedAtGithub { get; set; }
        public DateTime UpdatedAtGithub { get; set; }
        public DateTime? MergedAtGithub { get; set; }
        public DateTime? ClosedAtGithub { get; set; }

        public string? PrUrl { get; set; }

        public DateTime CreatedAt { get; set; }   // default SYSUTCDATETIME()

        // Navigation
        public Repository Repository { get; set; } = default!;
        public ICollection<SnapshotPullRequest> SnapshotPullRequests { get; set; } = new List<SnapshotPullRequest>();
    }
}
