using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Abstractions
{
    public interface IApplicationDbContext
    {
        DbSet<Group> Groups { get; }
        DbSet<Project> Projects { get; }
        DbSet<SyncRun> SyncRuns { get; }
        DbSet<Snapshot> Snapshots { get; }
        DbSet<JiraIssue> JiraIssues { get; }
        DbSet<IssueCommitLink> IssueCommitLinks { get; }
        DbSet<GitHubCommit> GitHubCommits { get; }
        DbSet<User> Users { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
