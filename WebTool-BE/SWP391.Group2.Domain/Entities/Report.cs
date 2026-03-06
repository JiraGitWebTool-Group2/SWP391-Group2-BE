using System;
namespace SWP391.Group2.Domain.Entities
{
    public class Report
    {
        public int ReportId { get; set; }
        public int ProjectId { get; set; }
        public int CreatedByUserId { get; set; }
        public int? SnapshotId { get; set; }

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Status { get; set; } = null!; // DRAFT | FINAL

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public Snapshot? Snapshot { get; set; }
    }
}
