using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Snapshots.Dtos
{
    public class SnapshotDailySummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalCommits { get; set; }
        public int DistinctContributors { get; set; }
        public int DistinctRepositories { get; set; }
    }
}
