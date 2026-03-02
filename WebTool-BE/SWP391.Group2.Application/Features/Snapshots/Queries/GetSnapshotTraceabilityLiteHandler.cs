using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Snapshots.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public sealed class GetSnapshotTraceabilityLiteHandler
    : IRequestHandler<GetSnapshotTraceabilityLiteQuery, SnapshotTraceabilityLiteDto>
    {
        private readonly IApplicationDbContext _db;

        // Regex chuẩn issue key: ABC-123, SWP391-10, PROJ2-9999...
        private static readonly Regex IssueKeyRegex =
            new(@"\b[A-Z][A-Z0-9]+-\d+\b", RegexOptions.Compiled);

        public GetSnapshotTraceabilityLiteHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SnapshotTraceabilityLiteDto> Handle(GetSnapshotTraceabilityLiteQuery request, CancellationToken ct)
        {
            // 1) snapshot tồn tại?
            var exists = await _db.Snapshots.AsNoTracking()
                .AnyAsync(s => s.SnapshotId == request.SnapshotId, ct);

            if (!exists)
                throw new KeyNotFoundException($"Snapshot {request.SnapshotId} not found.");

            // 2) lấy message commit thuộc snapshot (snapshot isolation)
            var messages = await (
                from sc in _db.SnapshotCommits.AsNoTracking()
                join c in _db.GitHubCommits.AsNoTracking()
                    on sc.CommitId equals c.CommitId
                where sc.SnapshotId == request.SnapshotId
                select c.Message
            ).ToListAsync(ct);

            var total = messages.Count;
            var commitsWithKey = 0;

            // key -> count occurrences across commits
            var keyCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var msg in messages)
            {
                if (string.IsNullOrWhiteSpace(msg))
                    continue;

                var matches = IssueKeyRegex.Matches(msg);
                if (matches.Count == 0) continue;

                // “commit có issue key” tính theo commit (không theo số lần match)
                commitsWithKey++;

                foreach (Match m in matches)
                {
                    var key = m.Value;

                    // Optional filter theo project key: SWP391-xxx
                    if (!string.IsNullOrWhiteSpace(request.ProjectKeyPrefix))
                    {
                        var prefix = request.ProjectKeyPrefix.Trim().ToUpperInvariant() + "-";
                        if (!key.StartsWith(prefix, StringComparison.Ordinal))
                            continue;
                    }

                    keyCounts[key] = keyCounts.TryGetValue(key, out var c) ? c + 1 : 1;
                }
            }

            var coverage = total == 0 ? 0 : Math.Round(commitsWithKey * 100.0 / total, 2);

            var stats = keyCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Take(Math.Clamp(request.Top, 1, 500))
                .Select(kv => new IssueKeyStatDto(kv.Key, kv.Value))
                .ToList();

            return new SnapshotTraceabilityLiteDto
            {
                SnapshotId = request.SnapshotId,
                TotalCommits = total,
                CommitsWithIssueKey = commitsWithKey,
                CoveragePercent = coverage,
                IssueKeyStats = stats
            };
        }
    }
}
