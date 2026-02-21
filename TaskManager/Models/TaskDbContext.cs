using Microsoft.EntityFrameworkCore;

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

            //Seed initial data for Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Role = "User" },
                new User { Id = 2, Name = "Bob Smith", Email = "bob@example.com", Role = "User" },
                new User { Id = 3, Name = "Charlie Lee", Email = "charlie@example.com", Role = "User" },
                new User
                {
                    Id = 4,
                    Name = "System Admin",
                    Email = "admin@taskmanager.local",
                    Role = "Admin",
                    PasswordSalt = new byte[] { 11, 22, 33, 44, 55, 66, 77, 88, 99, 110, 121, 132, 143, 154, 165, 176 },
                    PasswordHash = new byte[] { 225, 133, 129, 224, 237, 99, 105, 218, 18, 122, 242, 74, 140, 236, 50, 125, 236, 222, 55, 162, 86, 42, 229, 189, 130, 255, 193, 18, 175, 123, 137, 129 }
                },
                new User
                {
                    Id = 5,
                    Name = "Project Manager",
                    Email = "manager@taskmanager.local",
                    Role = "Manager",
                    PasswordSalt = new byte[] { 176, 165, 154, 143, 132, 121, 110, 99, 88, 77, 66, 55, 44, 33, 22, 11 },
                    PasswordHash = new byte[] { 7, 218, 90, 57, 224, 37, 143, 86, 4, 105, 85, 208, 199, 83, 93, 4, 125, 96, 71, 19, 114, 224, 21, 4, 80, 238, 217, 110, 175, 198, 224, 113 }
                }
            );

            //Seed initial data  for Projects
            modelBuilder.Entity<Project>().HasData(
                new Project { Id = 1, Name = "Website Redesign", Description = "Redesign corporate website", CreatedAt = new DateTime(2025, 8, 19) },
                new Project { Id = 2, Name = "Mobile App", Description = "Develop new mobile app" , CreatedAt = new DateTime(2025, 8, 19) }
            );

            //Seed initial data  for Tasks
            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem { Id = 1, Title = "Design Landing Page", Status = "Pending", Priority = "High", ProjectId = 1 , Description = "PlaceHolder"},
                new TaskItem { Id = 2, Title = "Setup Database", Status = "InProgress", Priority = "Medium", ProjectId = 2 , Description = "PlaceHolder" },
                new TaskItem { Id = 3, Title = "Implement Authentication", Status = "Pending", Priority = "High", ProjectId = 2 , Description = "PlaceHolder" }
            );

            //Seed initial data  for Assignments
            modelBuilder.Entity<TaskAssignment>().HasData(
                new TaskAssignment { TaskItemId = 1, UserId = 1 },
                new TaskAssignment { TaskItemId = 2, UserId = 2 },
                new TaskAssignment { TaskItemId = 3, UserId = 3 }
            );


        }
    }
}
