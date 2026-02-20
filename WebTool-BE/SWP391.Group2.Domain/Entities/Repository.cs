using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class Repository
    {
        public int RepoId { get; set; }
        public int ProjectId { get; set; }
        public string RepoName { get; set; } = default!;
        public string? RepoUrl { get; set; }
        public string? DefaultBranch { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Project Project { get; set; } = default!;
        public ICollection<GitHubCommit> GitHubCommits { get; set; } = new List<GitHubCommit>();
    }
}
