namespace SWP391.Group2.Application.Features.Projects.Dtos
{
    public record ProjectDto(
        int ProjectId,
        int GroupId,
        string ProjectName,
        string ProjectCode,

        string? Description,
        DateTime CreatedAt,
        string? Requirement
    );
}