using SWP391.Group2.Application.Abstractions.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SWP391.Group2.Infrastructure.Jobs
{
    public class BackgroundJobQueue : IBackgroundJobQueue
    {
        private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();

        public void EnqueueSyncRun(int syncRunId)
        {
            _channel.Writer.TryWrite(syncRunId);
        }

        public IAsyncEnumerable<int> DequeueAllAsync(CancellationToken ct)
            => _channel.Reader.ReadAllAsync(ct);
    }
}
