using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public int GroupId { get; set; }
        public string ProjectName { get; set; } = default!;
        public string? JiraProjectKey { get; set; }
        public string? GithubOrg { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        // Navigation
        public Group Group { get; set; } = default!;
        public ICollection<SyncRun> SyncRuns { get; set; } = new List<SyncRun>();
        public ICollection<JiraIssue> JiraIssues { get; set; } = new List<JiraIssue>();
        public ICollection<Repository> Repositories { get; set; } = new List<Repository>();
    }
}
