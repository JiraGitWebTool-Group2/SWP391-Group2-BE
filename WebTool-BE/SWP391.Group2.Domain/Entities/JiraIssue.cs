using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class JiraIssue
    {
        public int IssueId { get; set; }
        public int ProjectId { get; set; }
        public int? SprintId { get; set; }
        public int? AssigneeUserId { get; set; }
        public string IssueKey { get; set; } = default!;
        public string Summary { get; set; } = default!;
        public string? Description { get; set; }
        public string IssueType { get; set; } = default!;
        public string Priority { get; set; } = default!;
        public string Status { get; set; } = default!;
        public decimal? StoryPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? JiraUrl { get; set; }

        // Navigation
        public Project Project { get; set; } = default!;
    }

}
