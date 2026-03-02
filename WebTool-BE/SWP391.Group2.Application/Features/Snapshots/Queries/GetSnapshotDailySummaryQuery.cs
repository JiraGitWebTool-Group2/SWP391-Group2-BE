using MediatR;
using SWP391.Group2.Application.Features.Snapshots.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public record GetSnapshotDailySummaryQuery(int SnapshotId)
    : IRequest<List<SnapshotDailySummaryDto>>;
}
