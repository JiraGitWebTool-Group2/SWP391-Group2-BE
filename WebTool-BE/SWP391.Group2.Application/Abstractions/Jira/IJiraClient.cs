using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions.Jira
{
    public record JiraIssueDto(
        string IssueKey,
        string Summary,
        string? Description,

        string IssueType,
        string Priority,
        string Status,

        string? RawIssueType,
        string? RawPriority,
        string? RawStatus,

        decimal? StoryPoints,
        string Url,

        string? AssigneeAccountId,
        string? AssigneeDisplayName,

        DateTime? JiraCreatedAt,
        DateTime? JiraUpdatedAt,
        DateTime? JiraResolvedAt,

        string? ParentIssueKey,
        string? SprintExternalId,
        string? SprintName
    );

    public interface IJiraClient
    {
        Task<IReadOnlyList<JiraIssueDto>> SearchIssuesAsync(
            string baseUrl,
            string jql,
            string token,
            string? storyPointsFieldKey,
            string? sprintFieldKey,
            CancellationToken ct);
    }
}
