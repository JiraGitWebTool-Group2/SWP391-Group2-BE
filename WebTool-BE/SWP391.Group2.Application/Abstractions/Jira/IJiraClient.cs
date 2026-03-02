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
        decimal? StoryPoints,
        string Url,
        string? AssigneeAccountId
    );

    public interface IJiraClient
    {
        Task<IReadOnlyList<JiraIssueDto>> SearchIssuesAsync(
            string baseUrl,
            string jql,
            int maxResults,
            string token,
            CancellationToken ct
        );
    }
}
