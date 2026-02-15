using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Group = SWP391.Group2.Domain.Entities.Group;


namespace SWP391.Group2.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Group> Groups => Set<Group>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("Groups", "dbo");

                entity.HasKey(x => x.GroupId);

                entity.Property(x => x.GroupId)
                    .HasColumnName("group_id");

                entity.Property(x => x.GroupName)
                    .HasColumnName("group_name")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at");
            });
        }
    }
}
