using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.SyncRun.Commands
{
    public class TriggerSyncCommand : IRequest<int>
    {
        public int GroupId { get; set; }

        public string TriggerType { get; set; } = "MANUAL";
        public string ScopeType { get; set; } = "SPRINT";
        public int? SprintId { get; set; }

        public bool IncludeJira { get; set; } = true;
        public bool IncludeGithub { get; set; } = true;
    }
}
