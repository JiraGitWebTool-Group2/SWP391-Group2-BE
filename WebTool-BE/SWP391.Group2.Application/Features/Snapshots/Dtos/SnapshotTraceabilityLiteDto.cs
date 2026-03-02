using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public sealed class SnapshotTraceabilityLiteDto
    {
        public int SnapshotId { get; init; }
        public int TotalCommits { get; init; }
        public int CommitsWithIssueKey { get; init; }
        public double CoveragePercent { get; init; }
        public IReadOnlyList<IssueKeyStatDto> IssueKeyStats { get; init; } = Array.Empty<IssueKeyStatDto>();
    }

    public sealed record IssueKeyStatDto(string IssueKey, int Count);
}
