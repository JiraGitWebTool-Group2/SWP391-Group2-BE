using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions.GitHub
{
    public record GitHubCommitDto(
        string Sha,
        string Message,
        DateTime CommittedAt,
        string Url,
        string? AuthorLogin
    );

    public interface IGitHubClient
    {
        Task<IReadOnlyList<GitHubCommitDto>> GetCommitsAsync(
            string org,
            string repoName,
            DateTime fromUtc,
            DateTime toUtc,
            string token,
            CancellationToken ct
        );

        Task<IReadOnlyList<GitHubPullRequestDto>> GetPullRequestsAsync(
            string org,
            string repoName,
            DateTime fromUtc,
            DateTime toUtc,
            string token,
            CancellationToken ct);
    }
}
