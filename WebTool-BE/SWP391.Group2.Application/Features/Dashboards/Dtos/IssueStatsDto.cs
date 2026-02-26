using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Dashboards.Dtos
{
    public record IssueStatsDto(
        int Total,
        Dictionary<string, int> ByStatus,
        Dictionary<string, int> ByType,
        Dictionary<string, int> ByPriority
    );
}
