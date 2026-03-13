namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public class SnapshotSummaryDto
    {
        public int SnapshotId { get; set; }
        public int TotalCommits { get; set; }
        public int DistinctRepositories { get; set; }
        public int DistinctContributors { get; set; }
    }
}