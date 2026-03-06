namespace SWP391.Group2.Domain.Entities
{
    public class SrsDocument
    {
        public int SrsId { get; set; }
        public int ProjectId { get; set; }
        public int CreatedByUserId { get; set; }
        public int Version { get; set; }
        public string ScopeType { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
    }
}