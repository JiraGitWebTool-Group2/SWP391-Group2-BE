using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? PasswordHash { get; set; }
        public string? Provider { get; set; } // LOCAL/GOOGLE/GITHUB
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string System_Role { get; set; } = "STUDENT";

        public string? ProviderUserId { get; set; } // Google "sub"
        // Navigation
        public ICollection<GitHubCommit> GitHubCommits { get; set; } = new List<GitHubCommit>();

        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    }
}
