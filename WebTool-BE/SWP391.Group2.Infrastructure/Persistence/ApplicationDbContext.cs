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
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<SrsDocument> SrsDocuments => Set<SrsDocument>();
        public DbSet<Semester> Semesters => Set<Semester>();
        public DbSet<Class> Classes => Set<Class>();
        public DbSet<ClassStudent> ClassStudents => Set<ClassStudent>();

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserGroup> UserGroups => Set<UserGroup>();

        public DbSet<GitHubPullRequest> GitHubPullRequests => Set<GitHubPullRequest>();
        public DbSet<SnapshotPullRequest> SnapshotPullRequests => Set<SnapshotPullRequest>();

        public DbSet<Sprint> Sprints => Set<Sprint>();

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

                entity.Property(x => x.SystemRole)
                        .HasColumnName("system_role")
                        .HasMaxLength(20)
                        .IsRequired();
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

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                entity.HasKey(x => x.RoleId);

                entity.Property(x => x.RoleId)
                    .HasColumnName("role_id");

                entity.Property(x => x.RoleName)
                    .HasColumnName("role_name")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // UserGroup entity configuration
            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("UserGroups");

                // Composite key using UserId and GroupId
                entity.HasKey(x => new { x.UserId, x.GroupId });

                // Map properties to database columns
                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.GroupId).HasColumnName("group_id");
                entity.Property(x => x.RoleId).HasColumnName("role_id");
                entity.Property(x => x.IsActive).HasColumnName("is_active");
                entity.Property(x => x.JoinedAt).HasColumnName("joined_at");
                //entity.Property(x => x.CreatedAt).HasColumnName("created_at");

                // Foreign key relationships
                //entity.HasOne(x => x.User)
                //    .WithMany()
                //    .HasForeignKey(x => x.UserId)
                //    .OnDelete(DeleteBehavior.Restrict);

                //entity.HasOne(x => x.Group)
                //    .WithMany()
                //    .HasForeignKey(x => x.GroupId)
                //    .OnDelete(DeleteBehavior.Restrict);

                //entity.HasOne(x => x.Role)
                //    .WithMany()
                //    .HasForeignKey(x => x.RoleId)
                //    .OnDelete(DeleteBehavior.Restrict);
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

                entity.Property(x => x.TriggerType)
                    .HasColumnName("trigger_type")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.ScopeType)
                    .HasColumnName("scope_type")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.SprintId).HasColumnName("sprint_id");
                entity.Property(x => x.IncludeJira).HasColumnName("include_jira");
                entity.Property(x => x.IncludeGithub).HasColumnName("include_github");

                entity.Property(x => x.GithubFrom)
                    .HasColumnName("github_from")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.GithubTo)
                    .HasColumnName("github_to")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.SyncGithubCommits).HasColumnName("sync_github_commits");
                entity.Property(x => x.SyncGithubPullRequests).HasColumnName("sync_github_pull_requests");

                entity.Property(x => x.GithubSyncMode)
                    .HasColumnName("github_sync_mode")
                    .HasMaxLength(20);

                entity.Property(x => x.RunStatus)
                    .HasColumnName("run_status")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.StartedAt)
                    .HasColumnName("started_at")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.FinishedAt)
                    .HasColumnName("finished_at")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.Notes)
                    .HasColumnName("notes")
                    .HasMaxLength(1000);

                entity.HasIndex(x => x.ProjectId)
                    .HasDatabaseName("IX_SyncRuns_project_id");

                entity.HasIndex(x => x.SprintId)
                    .HasDatabaseName("IX_SyncRuns_sprint_id");

                entity.HasOne(x => x.Project)
                    .WithMany(p => p.SyncRuns)
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.TriggeredByUser)
                    .WithMany()
                    .HasForeignKey(x => x.TriggeredByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Sprint)
                    .WithMany(s => s.SyncRuns)
                    .HasForeignKey(x => x.SprintId)
                    .OnDelete(DeleteBehavior.Restrict);
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

                entity.Property(x => x.IssueKey)
                    .HasColumnName("issue_key")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Summary)
                    .HasColumnName("summary")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasColumnName("description");

                entity.Property(x => x.IssueType)
                    .HasColumnName("issue_type")
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.Priority)
                    .HasColumnName("priority")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasColumnName("status")
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(x => x.RawIssueType)
                    .HasColumnName("raw_issue_type")
                    .HasMaxLength(100);

                entity.Property(x => x.RawPriority)
                    .HasColumnName("raw_priority")
                    .HasMaxLength(100);

                entity.Property(x => x.RawStatus)
                    .HasColumnName("raw_status")
                    .HasMaxLength(100);

                entity.Property(x => x.StoryPoints)
                    .HasColumnName("story_points")
                    .HasColumnType("decimal(5,2)");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.JiraCreatedAt)
                    .HasColumnName("jira_created_at")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.JiraUpdatedAt)
                    .HasColumnName("jira_updated_at")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.JiraResolvedAt)
                    .HasColumnName("jira_resolved_at")
                    .HasColumnType("datetime2(0)");

                entity.Property(x => x.JiraUrl)
                    .HasColumnName("jira_url")
                    .HasMaxLength(500);

                entity.Property(x => x.JiraAssigneeAccountId)
                    .HasColumnName("jira_assignee_account_id")
                    .HasMaxLength(255);

                entity.Property(x => x.JiraAssigneeDisplayName)
                    .HasColumnName("jira_assignee_display_name")
                    .HasMaxLength(255);

                entity.Property(x => x.ParentIssueKey)
                    .HasColumnName("parent_issue_key")
                    .HasMaxLength(50);

                entity.HasIndex(x => x.ProjectId)
                    .HasDatabaseName("IX_JiraIssues_project_id");

                entity.HasIndex(x => x.SprintId)
                    .HasDatabaseName("IX_JiraIssues_sprint_id");

                entity.HasIndex(x => x.AssigneeUserId)
                    .HasDatabaseName("IX_JiraIssues_assignee");

                entity.HasIndex(x => new { x.ProjectId, x.IssueKey })
                    .IsUnique()
                    .HasDatabaseName("UQ_JiraIssues_project_key");

                entity.HasIndex(x => x.JiraUpdatedAt)
                    .HasDatabaseName("IX_JiraIssues_jira_updated_at");

                entity.HasIndex(x => new { x.ProjectId, x.ParentIssueKey })
                    .HasDatabaseName("IX_JiraIssues_parent_issue_key");

                entity.HasOne(x => x.Project)
                    .WithMany(p => p.JiraIssues)
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Sprint)
                    .WithMany(s => s.JiraIssues)
                    .HasForeignKey(x => x.SprintId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.AssigneeUser)
                    .WithMany()
                    .HasForeignKey(x => x.AssigneeUserId)
                    .OnDelete(DeleteBehavior.Restrict);
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
                e.ToTable("ProjectIntegrations", "dbo");

                e.HasKey(x => x.IntegrationId);

                e.Property(x => x.IntegrationId)
                    .HasColumnName("integration_id");

                e.Property(x => x.ProjectId)
                    .HasColumnName("project_id")
                    .IsRequired();

                e.Property(x => x.Provider)
                    .HasColumnName("provider")
                    .HasMaxLength(10)
                    .IsRequired();

                e.Property(x => x.BaseUrl)
                    .HasColumnName("base_url")
                    .HasMaxLength(300);

                e.Property(x => x.ProjectKey)
                    .HasColumnName("project_key")
                    .HasMaxLength(50);

                e.Property(x => x.Org)
                    .HasColumnName("org")
                    .HasMaxLength(200);

                e.Property(x => x.TokenEncrypted)
                    .HasColumnName("token_encrypted")
                    .HasMaxLength(1000);

                e.Property(x => x.CreatedByUserId)
                    .HasColumnName("created_by_user_id");

                e.Property(x => x.LinkedAccount)
                    .HasColumnName("linked_account")
                    .HasMaxLength(255);

                e.Property(x => x.VisibilityStatus)
                    .HasColumnName("visibility_status")
                    .HasMaxLength(20);

                e.Property(x => x.LastVerifiedAt)
                    .HasColumnName("last_verified_at")
                    .HasColumnType("datetime2(0)");

                e.Property(x => x.VerificationNote)
                    .HasColumnName("verification_note")
                    .HasMaxLength(500);

                e.Property(x => x.JiraStoryPointsFieldKey)
                    .HasColumnName("jira_story_points_field_key")
                    .HasMaxLength(100);

                e.Property(x => x.JiraSprintFieldKey)
                    .HasColumnName("jira_sprint_field_key")
                    .HasMaxLength(100);

                e.Property(x => x.JiraBoardId)
                    .HasColumnName("jira_board_id")
                    .HasMaxLength(50);

                e.Property(x => x.LastJiraSyncAt)
                    .HasColumnName("last_jira_sync_at")
                    .HasColumnType("datetime2(0)");

                e.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime2(0)");

                e.Property(x => x.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("datetime2(0)");

                e.HasIndex(x => x.ProjectId)
                    .HasDatabaseName("IX_ProjectIntegrations_project_id");

                e.HasIndex(x => new { x.ProjectId, x.Provider })
                    .IsUnique()
                    .HasDatabaseName("UQ_ProjectIntegrations");

                e.HasOne(x => x.Project)
                    .WithMany()
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
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

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Reports");

                entity.HasKey(x => x.ReportId);

                entity.Property(x => x.ReportId)
                      .HasColumnName("report_id");

                entity.Property(x => x.ProjectId)
                      .HasColumnName("project_id");

                entity.Property(x => x.CreatedByUserId)
                      .HasColumnName("created_by_user_id");

                entity.Property(x => x.SnapshotId)
                      .HasColumnName("snapshot_id");

                entity.Property(x => x.Title)
                      .HasColumnName("title")
                      .HasMaxLength(300)
                      .IsRequired();

                entity.Property(x => x.Content)
                      .HasColumnName("content")
                      .IsRequired();

                entity.Property(x => x.Status)
                      .HasColumnName("status")
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(x => x.CreatedAt)
                      .HasColumnName("created_at");

                entity.Property(x => x.UpdatedAt)
                      .HasColumnName("updated_at");

                entity.HasOne(x => x.Project)
                      .WithMany()
                      .HasForeignKey(x => x.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(x => x.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Snapshot)
                      .WithMany()
                      .HasForeignKey(x => x.SnapshotId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SrsDocument>(entity =>
            {
                entity.ToTable("SrsDocuments");

                entity.HasKey(x => x.SrsId);

                entity.Property(x => x.SrsId)
                    .HasColumnName("srs_id");

                entity.Property(x => x.ProjectId)
                    .HasColumnName("project_id");

                entity.Property(x => x.CreatedByUserId)
                    .HasColumnName("created_by_user_id");

                entity.Property(x => x.Version)
                    .HasColumnName("version");

                entity.Property(x => x.ScopeType)
                    .HasColumnName("scope_type")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.Title)
                    .HasColumnName("title")
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(x => x.Content)
                    .HasColumnName("content")
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at");

                entity.HasOne(x => x.Project)
                    .WithMany()
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ProjectId)
                    .HasDatabaseName("IX_SrsDocuments_project_id");

                entity.HasIndex(x => new { x.ProjectId, x.Version })
                    .IsUnique()
                    .HasDatabaseName("UQ_SrsDocuments_project_version");
            });

            modelBuilder.Entity<Semester>(entity =>
            {
                entity.ToTable("Semesters", "dbo");

                entity.HasKey(x => x.SemesterId);

                entity.Property(x => x.SemesterId).HasColumnName("semester_id");
                entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
                //entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(x => x.StartDate).HasColumnName("start_date");
                entity.Property(x => x.EndDate).HasColumnName("end_date");
                entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("Classes", "dbo");

                entity.HasKey(x => x.ClassId);

                entity.Property(x => x.ClassId).HasColumnName("class_id");
                entity.Property(x => x.SemesterId).HasColumnName("semester_id");
                entity.Property(x => x.ClassCode).HasColumnName("class_code").HasMaxLength(50).IsRequired();
                //entity.Property(x => x.CourseCode).HasColumnName("course_code").HasMaxLength(50).IsRequired();
                //entity.Property(x => x.ClassName).HasColumnName("class_name").HasMaxLength(200);
                entity.Property(x => x.LecturerUserId).HasColumnName("lecturer_user_id");
                entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(x => x.SemesterId);

                entity.HasOne(x => x.Semester)
                    .WithMany()
                    .HasForeignKey(x => x.SemesterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.SemesterId, x.ClassCode })
                    .IsUnique();
            });

            modelBuilder.Entity<ClassStudent>(entity =>
            {
                entity.ToTable("ClassStudents", "dbo");

                entity.HasKey(x => x.ClassStudentId);

                entity.Property(x => x.ClassStudentId).HasColumnName("class_student_id");
                entity.Property(x => x.ClassId).HasColumnName("class_id");
                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.JoinedAt).HasColumnName("joined_at");
                entity.Property(x => x.IsActive).HasColumnName("is_active");

                entity.HasIndex(x => x.ClassId);
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.ClassId, x.UserId }).IsUnique();

                entity.HasOne(x => x.Class)
                    .WithMany()
                    .HasForeignKey(x => x.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GitHubPullRequest>(entity =>
            {
                entity.ToTable("GitHubPullRequests", "dbo");
                entity.HasKey(x => x.PullRequestId);

                entity.Property(x => x.PullRequestId).HasColumnName("pull_request_id");
                entity.Property(x => x.RepoId).HasColumnName("repo_id");

                entity.Property(x => x.PrNumber)
                    .HasColumnName("pr_number")
                    .IsRequired();

                entity.Property(x => x.Title)
                    .HasColumnName("title")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasColumnName("description");

                entity.Property(x => x.State)
                    .HasColumnName("state")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(x => x.AuthorLogin)
                    .HasColumnName("author_login")
                    .HasMaxLength(255);

                entity.Property(x => x.CreatedAtGithub).HasColumnName("created_at_github");
                entity.Property(x => x.UpdatedAtGithub).HasColumnName("updated_at_github");
                entity.Property(x => x.MergedAtGithub).HasColumnName("merged_at_github");
                entity.Property(x => x.ClosedAtGithub).HasColumnName("closed_at_github");

                entity.Property(x => x.PrUrl)
                    .HasColumnName("pr_url")
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(x => new { x.RepoId, x.PrNumber })
                    .IsUnique()
                    .HasDatabaseName("UQ_GitHubPullRequests_Repo_PrNumber");

                entity.HasIndex(x => new { x.RepoId, x.CreatedAtGithub })
                    .HasDatabaseName("IX_GitHubPullRequests_repo_id_created_at_github");

                entity.HasIndex(x => new { x.RepoId, x.UpdatedAtGithub })
                    .HasDatabaseName("IX_GitHubPullRequests_repo_id_updated_at_github");

                entity.HasOne(x => x.Repository)
                    .WithMany()
                    .HasForeignKey(x => x.RepoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SnapshotPullRequest>(entity =>
            {
                entity.ToTable("SnapshotPullRequests", "dbo");
                entity.HasKey(x => x.SnapshotPullRequestId);

                entity.Property(x => x.SnapshotPullRequestId).HasColumnName("snapshot_pull_request_id");
                entity.Property(x => x.SnapshotId).HasColumnName("snapshot_id");
                entity.Property(x => x.PullRequestId).HasColumnName("pull_request_id");

                entity.HasIndex(x => x.SnapshotId)
                    .HasDatabaseName("IX_SnapshotPullRequests_snapshot_id");

                entity.HasIndex(x => new { x.SnapshotId, x.PullRequestId })
                    .IsUnique()
                    .HasDatabaseName("UQ_SnapshotPullRequests");

                entity.HasOne(x => x.Snapshot)
                    .WithMany(x => x.SnapshotPullRequests)
                    .HasForeignKey(x => x.SnapshotId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PullRequest)
                    .WithMany(x => x.SnapshotPullRequests)
                    .HasForeignKey(x => x.PullRequestId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Sprint>(entity =>
            {
                entity.ToTable("Sprints", "dbo");
                entity.HasKey(x => x.SprintId);

                entity.Property(x => x.SprintId).HasColumnName("sprint_id");

                entity.Property(x => x.ProjectId)
                    .HasColumnName("project_id");

                entity.Property(x => x.JiraSprintId)
                    .HasColumnName("jira_sprint_id")
                    .HasMaxLength(50);

                entity.Property(x => x.SprintName)
                    .HasColumnName("sprint_name")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("date");

                entity.Property(x => x.EndDate)
                    .HasColumnName("end_date")
                    .HasColumnType("date");

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime2(0)");

                entity.HasIndex(x => x.ProjectId)
                    .HasDatabaseName("IX_Sprints_project_id");

                entity.HasIndex(x => new { x.ProjectId, x.SprintName })
                    .IsUnique()
                    .HasDatabaseName("UQ_Sprints_project_name");

                entity.HasIndex(x => new { x.ProjectId, x.JiraSprintId })
                    .IsUnique()
                    .HasFilter("[jira_sprint_id] IS NOT NULL")
                    .HasDatabaseName("UX_Sprints_project_jira_sprint_id");

                entity.HasOne(x => x.Project)
                    .WithMany()
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
