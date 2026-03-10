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
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<ProjectIntegration> ProjectIntegrations { get; }
        DbSet<Repository> Repositories { get; }
        DbSet<SnapshotCommit> SnapshotCommits { get; }
        DbSet<Report> Reports { get; }
        DbSet<SrsDocument> SrsDocuments { get; }
        DbSet<Semester> Semesters { get; }
        DbSet<Class> Classes { get; }
        DbSet<ClassStudent> ClassStudents { get; }
        DbSet<UserGroup> UserGroups { get; }
        DbSet<Role> Roles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
