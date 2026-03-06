namespace SWP391.Group2.Application.Features.Reports.Dtos
{
    public class ReportDto
    {
        public int ReportId { get; set; }
        public int ProjectId { get; set; }
        public int CreatedByUserId { get; set; }
        public int? SnapshotId { get; set; }

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}