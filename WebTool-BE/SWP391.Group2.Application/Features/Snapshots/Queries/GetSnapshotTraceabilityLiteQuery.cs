using MediatR;
using SWP391.Group2.Application.Features.Snapshots.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public sealed record GetSnapshotTraceabilityLiteQuery(
        int SnapshotId,
        string? ProjectKeyPrefix = null, // optional: "SWP391" để filter key
        int Top = 50                    // lấy top N issue keys
    ) : IRequest<SnapshotTraceabilityLiteDto>;
}
