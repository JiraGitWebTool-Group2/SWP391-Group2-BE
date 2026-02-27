using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Domain.Entities;
using SWP391.Group2.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SWP391.Group2.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Group> Groups => Set<Group>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Project> Projects => Set<Project>();
 
        public DbSet<SyncRun> SyncRuns => Set<SyncRun>();
        public DbSet<Snapshot> Snapshots => Set<Snapshot>();
        public DbSet<JiraIssue> JiraIssues => Set<JiraIssue>();
        public DbSet<Repository> Repositories => Set<Repository>();
        public DbSet<GitHubCommit> GitHubCommits => Set<GitHubCommit>();
        public DbSet<IssueCommitLink> IssueCommitLinks => Set<IssueCommitLink>();

        public DbSet<SnapshotCommit> SnapshotCommits => Set<SnapshotCommit>();

        public DbSet<ProjectIntegration> ProjectIntegrations => Set<ProjectIntegration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("Groups", "dbo");
                entity.HasKey(x => x.GroupId);

                entity.Property(x => x.GroupId).HasColumnName("group_id");
                entity.Property(x => x.GroupName).HasColumnName("group_name").HasMaxLength(200).IsRequired();
                entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", "dbo");
                entity.HasKey(x => x.UserId);

                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
                entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
                entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
                entity.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(30);
                entity.Property(x => x.IsActive).HasColumnName("is_active");
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                entity.Property(x => x.ProviderUserId).HasColumnName("provider_user_id").HasMaxLength(200);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens", "dbo");
                entity.HasKey(x => x.RefreshTokenId);

                entity.Property(x => x.RefreshTokenId).HasColumnName("refresh_token_id");
                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
                entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
                entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects", "dbo");

                entity.HasKey(x => x.ProjectId);

                entity.Property(x => x.ProjectId).HasColumnName("project_id");
                entity.Property(x => x.GroupId).HasColumnName("group_id");
                entity.Property(x => x.ProjectName).HasColumnName("project_name").HasMaxLength(200).IsRequired();
                entity.Property(x => x.JiraProjectKey).HasColumnName("jira_project_key").HasMaxLength(50);
                entity.Property(x => x.GithubOrg).HasColumnName("github_org").HasMaxLength(200);
                entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<SyncRun>(entity =>
            {
                entity.ToTable("SyncRuns", "dbo");
                entity.HasKey(x => x.SyncRunId);

                entity.Property(x => x.SyncRunId).HasColumnName("sync_run_id");
                entity.Property(x => x.ProjectId).HasColumnName("project_id");
                entity.Property(x => x.TriggeredByUserId).HasColumnName("triggered_by_user_id");
                entity.Property(x => x.TriggerType).HasColumnName("trigger_type").HasMaxLength(10).IsRequired();
                entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(10).IsRequired();
                entity.Property(x => x.SprintId).HasColumnName("sprint_id");
                entity.Property(x => x.IncludeJira).HasColumnName("include_jira");
                entity.Property(x => x.IncludeGithub).HasColumnName("include_github");
                entity.Property(x => x.RunStatus).HasColumnName("run_status").HasMaxLength(10).IsRequired();
                entity.Property(x => x.StartedAt).HasColumnName("started_at");
                entity.Property(x => x.FinishedAt).HasColumnName("finished_at");
                entity.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);

                entity.HasOne(x => x.Project)
                    .WithMany(p => p.SyncRuns)
                    .HasForeignKey(x => x.ProjectId);
            });

            modelBuilder.Entity<Snapshot>(entity =>
            {
                entity.ToTable("Snapshots", "dbo");
                entity.HasKey(x => x.SnapshotId);

                entity.Property(x => x.SnapshotId).HasColumnName("snapshot_id");
                entity.Property(x => x.SyncRunId).HasColumnName("sync_run_id");
                entity.Property(x => x.CapturedAt).HasColumnName("captured_at");
                entity.Property(x => x.Label).HasColumnName("label").HasMaxLength(200);

                entity.HasOne(x => x.SyncRun)
                    .WithMany(sr => sr.Snapshots)
                    .HasForeignKey(x => x.SyncRunId);
            });

            modelBuilder.Entity<JiraIssue>(entity =>
            {
                entity.ToTable("JiraIssues", "dbo");
                entity.HasKey(x => x.IssueId);

                entity.Property(x => x.IssueId).HasColumnName("issue_id");
                entity.Property(x => x.ProjectId).HasColumnName("project_id");
                entity.Property(x => x.SprintId).HasColumnName("sprint_id");
                entity.Property(x => x.AssigneeUserId).HasColumnName("assignee_user_id");
                entity.Property(x => x.IssueKey).HasColumnName("issue_key").HasMaxLength(50).IsRequired();
                entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(500).IsRequired();
                entity.Property(x => x.Description).HasColumnName("description");
                entity.Property(x => x.IssueType).HasColumnName("issue_type").HasMaxLength(30).IsRequired();
                entity.Property(x => x.Priority).HasColumnName("priority").HasMaxLength(20).IsRequired();
                entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
                entity.Property(x => x.StoryPoints).HasColumnName("story_points");
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
                entity.Property(x => x.JiraUrl).HasColumnName("jira_url").HasMaxLength(500);

                entity.HasOne(x => x.Project)
                    .WithMany(p => p.JiraIssues)
                    .HasForeignKey(x => x.ProjectId);
            });

            modelBuilder.Entity<Repository>(entity =>
            {
                entity.ToTable("Repositories", "dbo");
                entity.HasKey(x => x.RepoId);

                entity.Property(x => x.RepoId).HasColumnName("repo_id");
                entity.Property(x => x.ProjectId).HasColumnName("project_id");
                entity.Property(x => x.RepoName).HasColumnName("repo_name").HasMaxLength(200).IsRequired();
                entity.Property(x => x.RepoUrl).HasColumnName("repo_url").HasMaxLength(500);
                entity.Property(x => x.DefaultBranch).HasColumnName("default_branch").HasMaxLength(100);
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");

                entity.HasOne(x => x.Project)
                    .WithMany(p => p.Repositories)
                    .HasForeignKey(x => x.ProjectId);
            });

            modelBuilder.Entity<GitHubCommit>(entity =>
            {
                entity.ToTable("GitHubCommits", "dbo");
                entity.HasKey(x => x.CommitId);

                entity.Property(x => x.CommitId).HasColumnName("commit_id");
                entity.Property(x => x.RepoId).HasColumnName("repo_id");
                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.CommitHash).HasColumnName("commit_hash").HasMaxLength(80).IsRequired();
                entity.Property(x => x.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
                entity.Property(x => x.CommittedAt).HasColumnName("committed_at");
                entity.Property(x => x.CommitUrl).HasColumnName("commit_url").HasMaxLength(500);

                entity.HasOne(x => x.Repository)
                    .WithMany(r => r.GitHubCommits)
                    .HasForeignKey(x => x.RepoId);

                entity.HasOne(x => x.User)
                    .WithMany(u => u.GitHubCommits)
                    .HasForeignKey(x => x.UserId);
            });

            modelBuilder.Entity<IssueCommitLink>(entity =>
            {
                entity.ToTable("IssueCommitLinks", "dbo");
                entity.HasKey(x => x.LinkId);

                entity.Property(x => x.LinkId).HasColumnName("link_id");
                entity.Property(x => x.SnapshotId).HasColumnName("snapshot_id");
                entity.Property(x => x.IssueId).HasColumnName("issue_id");
                entity.Property(x => x.CommitId).HasColumnName("commit_id");

                entity.HasOne(x => x.Snapshot)
                    .WithMany(s => s.IssueCommitLinks)
                    .HasForeignKey(x => x.SnapshotId);

                entity.HasOne(x => x.JiraIssue)
                    .WithMany()
                    .HasForeignKey(x => x.IssueId);

                entity.HasOne(x => x.GitHubCommit)
                    .WithMany()
                    .HasForeignKey(x => x.CommitId);
            });

            modelBuilder.Entity<ProjectIntegration>(e =>
            {
                e.ToTable("ProjectIntegrations");

                e.HasKey(x => x.IntegrationId);

                e.Property(x => x.IntegrationId).HasColumnName("integration_id");
                e.Property(x => x.ProjectId).HasColumnName("project_id");

                e.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(10);

                e.Property(x => x.BaseUrl).HasColumnName("base_url").HasMaxLength(300);
                e.Property(x => x.ProjectKey).HasColumnName("project_key").HasMaxLength(50);
                e.Property(x => x.Org).HasColumnName("org").HasMaxLength(200);

                e.Property(x => x.TokenEncrypted).HasColumnName("token_encrypted").HasMaxLength(1000);

                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(x => new { x.ProjectId, x.Provider }).IsUnique();

                // FK
                // Nếu bạn đã có entity Project thì add navigation sau, chưa có cũng không sao.
            });

            modelBuilder.Entity<SnapshotCommit>(e =>
            {
                e.ToTable("SnapshotCommits");
                e.HasKey(x => x.SnapshotCommitId);

                e.Property(x => x.SnapshotCommitId).HasColumnName("snapshot_commit_id");
                e.Property(x => x.SnapshotId).HasColumnName("snapshot_id");
                e.Property(x => x.CommitId).HasColumnName("commit_id");

                e.HasIndex(x => new { x.SnapshotId, x.CommitId }).IsUnique();
            });
        }
    }
}
