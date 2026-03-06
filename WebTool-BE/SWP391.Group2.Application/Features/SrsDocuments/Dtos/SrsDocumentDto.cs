namespace SWP391.Group2.Application.Features.SrsDocuments.Dtos
{
    public class SrsDocumentDto
    {
        public int SrsId { get; set; }
        public int ProjectId { get; set; }
        public int CreatedByUserId { get; set; }
        public int Version { get; set; }
        public string ScopeType { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}