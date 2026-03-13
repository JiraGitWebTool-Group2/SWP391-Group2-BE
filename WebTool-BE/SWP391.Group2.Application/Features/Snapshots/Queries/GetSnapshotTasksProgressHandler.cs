using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Snapshots.Dtos;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public class GetSnapshotTasksProgressHandler
        : IRequestHandler<GetSnapshotTasksProgressQuery, SnapshotTasksProgressDto>
    {
        private readonly IApplicationDbContext _db;

        public GetSnapshotTasksProgressHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SnapshotTasksProgressDto> Handle(
            GetSnapshotTasksProgressQuery request,
            CancellationToken cancellationToken)
        {
            var snapshotExists = await _db.Snapshots
                .AsNoTracking()
                .AnyAsync(s => s.SnapshotId == request.SnapshotId, cancellationToken);

            if (!snapshotExists)
                throw new KeyNotFoundException("Snapshot not found.");

            var issues = await (
                from l in _db.IssueCommitLinks.AsNoTracking()
                join i in _db.JiraIssues.AsNoTracking()
                    on l.IssueId equals i.IssueId
                where l.SnapshotId == request.SnapshotId
                select new
                {
                    i.IssueId,
                    i.Status
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            int totalTasks = issues.Count;

            int todoTasks = issues.Count(x => IsTodoStatus(x.Status));
            int inProgressTasks = issues.Count(x => IsInProgressStatus(x.Status));
            int doneTasks = issues.Count(x => IsDoneStatus(x.Status));

            decimal completionRate = totalTasks == 0
                ? 0
                : Math.Round((decimal)doneTasks * 100 / totalTasks, 2);

            return new SnapshotTasksProgressDto
            {
                SnapshotId = request.SnapshotId,
                TotalTasks = totalTasks,
                TodoTasks = todoTasks,
                InProgressTasks = inProgressTasks,
                DoneTasks = doneTasks,
                CompletionRate = completionRate
            };
        }

        private static bool IsTodoStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return true;

            var s = status.Trim().ToUpperInvariant();

            return s is "TODO"
                or "TO DO"
                or "OPEN"
                or "BACKLOG"
                or "NEW";
        }

        private static bool IsInProgressStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;

            var s = status.Trim().ToUpperInvariant();

            return s is "IN_PROGRESS"
                or "IN PROGRESS"
                or "DOING"
                or "ONGOING"
                or "WORKING";
        }

        private static bool IsDoneStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;

            var s = status.Trim().ToUpperInvariant();

            return s is "DONE"
                or "CLOSED"
                or "RESOLVED"
                or "COMPLETED";
        }
    }
}