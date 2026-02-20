
namespace SWP391.Group2.Application.Features.Groups.Dtos
{
    public record DashboardDto(
        int GroupId,
        int SnapshotId,
        DateTime CapturedAt,
        IssueStatsDto Issues,
        CommitStatsDto Commits
    );

    public record IssueStatsDto(
        int Total,
        Dictionary<string, int> ByStatus,
        Dictionary<string, int> ByType,
        Dictionary<string, int> ByPriority
    );

    public record CommitStatsDto(
        int TotalInSnapshot,
        int LinkedIssues,
        int Links,
        List<ContributorDto> TopContributors
    );

    public record ContributorDto(int? UserId, string FullName, int Commits);
}
