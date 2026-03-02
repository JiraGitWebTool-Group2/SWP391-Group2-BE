using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Sync.Services
{
    public static class IssueKeyExtractor
    {
        // SWP391-123, ABC-1, ABC12-999...
        private static readonly Regex Rx = new(@"\b[A-Z][A-Z0-9]+-\d+\b", RegexOptions.Compiled);

        public static IReadOnlyCollection<string> Extract(string? message)
            => Rx.Matches(message ?? "")
                 .Select(m => m.Value.ToUpperInvariant())
                 .Distinct()
                 .ToList();
    }
}
