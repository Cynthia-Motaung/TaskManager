using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Models
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TaskAssignment Many-to-Many
            modelBuilder.Entity<TaskAssignment>()
                .HasKey(t => new { t.UserId, t.TaskItemId });

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(t => t.User)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(t => t.TaskItem)
                .WithMany(ti => ti.TaskAssignments)
                .HasForeignKey(t => t.TaskItemId);

            // TaskDependency Many-to-Many
            modelBuilder.Entity<TaskDependency>()
                .HasKey(td => new { td.TaskItemId, td.DependsOnTaskId });

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.TaskItem)
                .WithMany(t => t.Dependencies)
                .HasForeignKey(td => td.TaskItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.DependsOnTask)
                .WithMany(t => t.DependedOnBy)
                .HasForeignKey(td => td.DependsOnTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
