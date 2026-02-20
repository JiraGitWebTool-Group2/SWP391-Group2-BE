using MediatR;
using SWP391.Group2.Application.Features.SyncRun.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.SyncRun.Queries
{
    public class GetSyncDetailQuery : IRequest<SyncDetailDto>
    {
        public int SyncId { get; set; }

        public GetSyncDetailQuery(int syncId)
        {
            SyncId = syncId;
        }
    }
}
