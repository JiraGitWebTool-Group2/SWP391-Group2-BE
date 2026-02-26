using MediatR;
using SWP391.Group2.Application.Features.Dashboards.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Dashboards.Queries
{
    public record GetDashboardQuery(int GroupId) : IRequest<DashboardDto>;
}
