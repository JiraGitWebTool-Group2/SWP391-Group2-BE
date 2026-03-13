using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Snapshots.Dtos;

namespace SWP391.Group2.Application.Features.Snapshots.Queries
{
    public class GetSnapshotMemberActivityHandler
        : IRequestHandler<GetSnapshotMemberActivityQuery, SnapshotMemberActivityDto>
    {
        private readonly IApplicationDbContext _db;

        public GetSnapshotMemberActivityHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SnapshotMemberActivityDto> Handle(
            GetSnapshotMemberActivityQuery request,
            CancellationToken cancellationToken)
        {
            var snapshotExists = await _db.Snapshots
                .AsNoTracking()
                .AnyAsync(s => s.SnapshotId == request.SnapshotId, cancellationToken);

            if (!snapshotExists)
                throw new KeyNotFoundException("Snapshot not found.");

            var taskActivities = await (
                from link in _db.IssueCommitLinks.AsNoTracking()
                join issue in _db.JiraIssues.AsNoTracking()
                    on link.IssueId equals issue.IssueId
                join user in _db.Users.AsNoTracking()
                    on issue.AssigneeUserId equals user.UserId
                where link.SnapshotId == request.SnapshotId
                      && issue.AssigneeUserId != null
                select new
                {
                    user.UserId,
                    user.FullName,
                    issue.IssueId
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            var commitActivities = await (
                from link in _db.IssueCommitLinks.AsNoTracking()
                join commit in _db.GitHubCommits.AsNoTracking()
                    on link.CommitId equals commit.CommitId
                join user in _db.Users.AsNoTracking()
                    on commit.UserId equals user.UserId
                where link.SnapshotId == request.SnapshotId
                      && commit.UserId != null
                select new
                {
                    user.UserId,
                    user.FullName,
                    commit.CommitId
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            var taskMap = taskActivities
                .GroupBy(x => new { x.UserId, x.FullName })
                .ToDictionary(
                    g => g.Key.UserId,
                    g => new
                    {
                        UserName = g.Key.FullName ?? string.Empty,
                        TaskCount = g.Select(x => x.IssueId).Distinct().Count()
                    });

            var commitMap = commitActivities
                .GroupBy(x => new { x.UserId, x.FullName })
                .ToDictionary(
                    g => g.Key.UserId,
                    g => new
                    {
                        UserName = g.Key.FullName ?? string.Empty,
                        CommitCount = g.Select(x => x.CommitId).Distinct().Count()
                    });

            var allUserIds = taskMap.Keys
                .Union(commitMap.Keys)
                .Distinct()
                .ToList();

            var members = allUserIds
                .Select(userId =>
                {
                    var userName = taskMap.ContainsKey(userId)
                        ? taskMap[userId].UserName
                        : commitMap[userId].UserName;

                    var taskCount = taskMap.ContainsKey(userId)
                        ? taskMap[userId].TaskCount
                        : 0;

                    var commitCount = commitMap.ContainsKey(userId)
                        ? commitMap[userId].CommitCount
                        : 0;

                    return new SnapshotMemberActivityItemDto
                    {
                        UserId = userId,
                        UserName = userName,
                        TaskCount = taskCount,
                        CommitCount = commitCount
                    };
                })
                .OrderByDescending(x => x.TotalActivity)
                .ThenBy(x => x.UserName)
                .ToList();

            return new SnapshotMemberActivityDto
            {
                SnapshotId = request.SnapshotId,
                Members = members
            };
        }
    }
}