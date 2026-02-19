using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SWP391.Group2.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Group> Groups => Set<Group>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Project> Projects => Set<Project>();

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
        }
    }
}
