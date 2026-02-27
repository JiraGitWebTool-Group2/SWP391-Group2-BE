using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions.Jobs
{
    public interface IBackgroundJobQueue
    {
        void EnqueueSyncRun(int syncRunId);
    }
}
