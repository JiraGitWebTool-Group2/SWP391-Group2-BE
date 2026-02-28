using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public sealed class SnapshotDailySummaryDto
    {
        public int SnapshotId { get; init; }
        public DateTime CapturedAt { get; init; }
        public IReadOnlyList<SnapshotDailyBucketDto> Days { get; init; } = Array.Empty<SnapshotDailyBucketDto>();
    }

    public sealed class SnapshotDailyBucketDto
    {
        // dùng DateOnly cho sạch, serialize JSON ổn trên .NET 8
        public DateOnly Date { get; init; }
        public int CommitCount { get; init; }
        public int ActiveRepoCount { get; init; }
        public IReadOnlyList<AuthorCountDto> TopAuthors { get; init; } = Array.Empty<AuthorCountDto>();
    }

    public sealed record AuthorCountDto(string Name, int Count);
}
