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

        // normalized values
        public string IssueType { get; set; } = default!;
        public string Priority { get; set; } = default!;
        public string Status { get; set; } = default!;

        // raw Jira values
        public string? RawIssueType { get; set; }
        public string? RawPriority { get; set; }
        public string? RawStatus { get; set; }

        public decimal? StoryPoints { get; set; }

        // local timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // real Jira timestamps
        public DateTime? JiraCreatedAt { get; set; }
        public DateTime? JiraUpdatedAt { get; set; }
        public DateTime? JiraResolvedAt { get; set; }

        public string? JiraUrl { get; set; }

        // assignee raw data
        public string? JiraAssigneeAccountId { get; set; }
        public string? JiraAssigneeDisplayName { get; set; }

        // hierarchy
        public string? ParentIssueKey { get; set; }

        // Navigation
        public Project Project { get; set; } = default!;
        public Sprint? Sprint { get; set; }
        public User? AssigneeUser { get; set; }
    }

}
