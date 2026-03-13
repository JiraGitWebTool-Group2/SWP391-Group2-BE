using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class Sprint
    {
        public int SprintId { get; set; }
        public int ProjectId { get; set; }

        public string? JiraSprintId { get; set; }
        public string SprintName { get; set; } = default!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Project Project { get; set; } = default!;
        public ICollection<JiraIssue> JiraIssues { get; set; } = new List<JiraIssue>();
        public ICollection<SyncRun> SyncRuns { get; set; } = new List<SyncRun>();
    }
}
