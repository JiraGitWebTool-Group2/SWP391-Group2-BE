using MediatR;
using SWP391.Group2.Application.Features.Snapshots.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public sealed record GetSnapshotDailySummaryQuery(int SnapshotId, int TzOffsetMinutes = 0)
    : IRequest<SnapshotDailySummaryDto>;
}
