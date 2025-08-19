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

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Alice Johnson", Email = "alice@example.com" },
                new User { Id = 2, Name = "Bob Smith", Email = "bob@example.com" },
                new User { Id = 3, Name = "Charlie Lee", Email = "charlie@example.com" }
            );

            modelBuilder.Entity<Project>().HasData(
                new Project { Id = 1, Name = "Website Redesign", Description = "Redesign corporate website" },
                new Project { Id = 2, Name = "Mobile App", Description = "Develop new mobile app" }
            );

            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem { Id = 1, Title = "Design Landing Page", Status = "Pending", Priority = "High", ProjectId = 1 },
                new TaskItem { Id = 2, Title = "Setup Database", Status = "InProgress", Priority = "Medium", ProjectId = 2 },
                new TaskItem { Id = 3, Title = "Implement Authentication", Status = "Pending", Priority = "High", ProjectId = 2 }
            );

            modelBuilder.Entity<TaskAssignment>().HasData(
                new TaskAssignment { TaskItemId = 1, UserId = 1 },
                new TaskAssignment { TaskItemId = 2, UserId = 2 },
                new TaskAssignment { TaskItemId = 3, UserId = 3 }
            );


        }
    }
}
