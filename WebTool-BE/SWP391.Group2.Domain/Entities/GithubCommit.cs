using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class GitHubCommit
    {
        public int CommitId { get; set; }
        public int RepoId { get; set; }
        public int? UserId { get; set; }
        public string CommitHash { get; set; } = default!;
        public string Message { get; set; } = default!;
        public DateTime CommittedAt { get; set; }
        public string? CommitUrl { get; set; }

        // Navigation
        public Repository Repository { get; set; } = default!;
        public User? User { get; set; }
    }
}
