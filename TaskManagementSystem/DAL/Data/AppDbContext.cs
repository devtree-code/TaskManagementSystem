using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DAL.Models;

namespace TaskManagementSystem.DAL.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserTask> Tasks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure UserTask entity
            builder.Entity<UserTask>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Description)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(t => t.TaskDate)
                    .IsRequired();

                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.TaskDate);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
