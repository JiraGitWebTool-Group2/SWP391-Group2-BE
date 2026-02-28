using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public record SnapshotCommitItemDto(
        int CommitId,
        string CommitHash,
        string Message,
        DateTime CommittedAt,
        string? CommitUrl,
        int RepoId,
        string RepoName
    );
}
