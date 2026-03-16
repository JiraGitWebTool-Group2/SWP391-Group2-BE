namespace SWP391.Group2.Api.Contracts.Projects
{
    public class UpdateProjectRequest
    {
        public string ProjectCode { get; set; } = default!;
        public string ProjectName { get; set; } = default!;
        public string? Description { get; set; }
        public string? Requirement { get; set; }
    }
}