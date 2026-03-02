using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public class TraceabilityItemDto
    {
        public int IssueId { get; set; }
        public string IssueKey { get; set; } = default!;
        public string Summary { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string Priority { get; set; } = default!;
        public string IssueType { get; set; } = default!;
        public decimal? StoryPoints { get; set; }
        public string? JiraUrl { get; set; }

        public int CommitCount { get; set; }
        public DateTime? LatestCommitAt { get; set; }
    }
}
