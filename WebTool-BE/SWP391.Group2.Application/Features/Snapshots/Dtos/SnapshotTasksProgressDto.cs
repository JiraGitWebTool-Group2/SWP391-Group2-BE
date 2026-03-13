namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public class SnapshotTasksProgressDto
    {
        public int SnapshotId { get; set; }
        public int TotalTasks { get; set; }
        public int TodoTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int DoneTasks { get; set; }
        public decimal CompletionRate { get; set; }
    }
}