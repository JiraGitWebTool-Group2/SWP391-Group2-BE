using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Dashboards.Dtos
{
    public record CommitStatsDto(
        int TotalInSnapshot,
        int LinkedIssues,
        int Links,
        List<ContributorDto> TopContributors
    );
}
