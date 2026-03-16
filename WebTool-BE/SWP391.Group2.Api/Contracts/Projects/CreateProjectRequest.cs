namespace SWP391.Group2.Api.Contracts.Projects
{
    namespace SWP391.Group2.Api.Contracts.Projects
    {
        public record CreateProjectRequest(
        string ProjectCode,
        string ProjectName,
        string? Description,
        string? Requirement
);
    }
}
