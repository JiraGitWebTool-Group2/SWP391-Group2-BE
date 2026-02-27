using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Repositories.Dtos
{
    public record RepositoryDto(
        int RepoId,
        int ProjectId,
        string RepoName,
        string? RepoUrl,
        string? DefaultBranch,
        DateTime CreatedAt
    );
}
