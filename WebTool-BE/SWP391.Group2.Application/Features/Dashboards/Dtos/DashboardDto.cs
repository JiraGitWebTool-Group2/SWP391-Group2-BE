using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Dashboards.Dtos
{
    public record DashboardDto(
        int GroupId,
        int SnapshotId,
        DateTime CapturedAt,
        IssueStatsDto Issues,
        CommitStatsDto Commits
    );

    

    


}
