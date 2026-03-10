using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class ProjectIntegration
    {
        public int IntegrationId { get; set; }
        public int ProjectId { get; set; }

        public string Provider { get; set; } = default!; // JIRA / GITHUB

        public string? BaseUrl { get; set; }
        public string? ProjectKey { get; set; }
        public string? Org { get; set; }
        public string? TokenEncrypted { get; set; }

        // New fields from updated DB
        public int? CreatedByUserId { get; set; }
        public string? LinkedAccount { get; set; }
        public string? VisibilityStatus { get; set; }
        public DateTime? LastVerifiedAt { get; set; }
        public string? VerificationNote { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Project Project { get; set; } = default!;
        public User? CreatedByUser { get; set; }
    }
}
