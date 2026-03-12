using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions.GitHub
{
    public record GitHubPullRequestDto(
        int Number,
        string Title,
        string? Description,
        string State,
        string? AuthorLogin,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? MergedAt,
        DateTime? ClosedAt,
        string Url
    );
}
