namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public class SnapshotMemberActivityDto
    {
        public int SnapshotId { get; set; }
        public List<SnapshotMemberActivityItemDto> Members { get; set; } = new();
    }
}