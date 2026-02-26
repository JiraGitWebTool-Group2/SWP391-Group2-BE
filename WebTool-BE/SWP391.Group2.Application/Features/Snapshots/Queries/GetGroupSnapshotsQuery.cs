using MediatR;
using SWP391.Group2.Application.Features.Snapshots.Dtos;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public record GetGroupSnapshotsQuery(int GroupId) : IRequest<List<SnapshotListItemDto>>;
}
