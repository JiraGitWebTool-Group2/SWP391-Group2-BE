using MediatR;
using SWP391.Group2.Application.Features.Sync.Dtos;


namespace SWP391.Group2.Application.Features.Sync.Queries
{
    public record GetSyncRunStatusQuery(int SyncRunId) : IRequest<SyncRunStatusDto>;
}
