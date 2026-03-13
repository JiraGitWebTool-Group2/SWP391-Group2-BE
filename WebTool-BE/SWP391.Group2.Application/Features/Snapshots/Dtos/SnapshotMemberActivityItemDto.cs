namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public class SnapshotMemberActivityItemDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public int CommitCount { get; set; }
        public int TotalActivity => TaskCount + CommitCount;
    }
}