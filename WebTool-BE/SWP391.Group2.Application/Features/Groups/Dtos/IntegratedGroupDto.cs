namespace SWP391.Group2.Application.Features.Groups.Dtos;

public sealed class IntegratedGroupDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public List<string> Integrations { get; set; } = new();
}