using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.SyncRun.Commands
{
    public class TriggerSyncHandler : IRequestHandler<TriggerSyncCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public TriggerSyncHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(
            TriggerSyncCommand request,
            CancellationToken cancellationToken)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.GroupId == request.GroupId, cancellationToken);

            if (project == null)
                throw new Exception("Project not found for this group.");

            //var syncRun = new SyncRun
            var syncRun = new SWP391.Group2.Domain.Entities.SyncRun

            {
                ProjectId = project.ProjectId,
                TriggerType = request.TriggerType,
                ScopeType = request.ScopeType,
                SprintId = request.SprintId,
                IncludeJira = request.IncludeJira,
                IncludeGithub = request.IncludeGithub,
                RunStatus = "RUNNING",
                StartedAt = DateTime.UtcNow
            };

            _context.SyncRuns.Add(syncRun);
            await _context.SaveChangesAsync(cancellationToken);

            return syncRun.SyncRunId;
        }
    }
}
