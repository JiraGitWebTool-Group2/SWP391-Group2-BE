using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SWP391.Group2.Application.Features.Sync;
using SWP391.Group2.Application.Features.Sync.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Infrastructure.Jobs
{
    public class SyncRunWorker : BackgroundService
    {
        private readonly BackgroundJobQueue _queue;
        private readonly IServiceProvider _sp;

        public SyncRunWorker(BackgroundJobQueue queue, IServiceProvider sp)
        {
            _queue = queue;
            _sp = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var syncRunId in _queue.DequeueAllAsync(stoppingToken))
            {
                using var scope = _sp.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                // chạy job thực tế bằng MediatR command
                await mediator.Send(new RunSyncJobCommand(syncRunId), stoppingToken);
            }
        }
    }
}
