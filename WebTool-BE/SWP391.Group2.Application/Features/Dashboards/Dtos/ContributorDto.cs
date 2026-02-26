using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Dashboards.Dtos
{
    public record ContributorDto(int? UserId, string FullName, int Commits);
}
