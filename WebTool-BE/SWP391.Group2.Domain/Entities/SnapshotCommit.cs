using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Domain.Entities
{
    public class SnapshotCommit
    {
        public int SnapshotCommitId { get; set; }
        public int SnapshotId { get; set; }
        public int CommitId { get; set; }
    }
}
