using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Models;

namespace TaskManager.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                db.Users.Add(new User { Id = 1, Name = "Test User", Email = "test.user@example.com" });
            }

            if (!db.Projects.Any())
            {
                db.Projects.Add(new Project { Id = 1, Name = "Test Project", Description = "Project for integration tests" });
            }

            if (!db.Tasks.Any())
            {
                db.Tasks.Add(new TaskItem
                {
                    Id = 1,
                    Title = "Seed Task",
                    Description = "Seed task for integration tests",
                    Status = "Pending",
                    Priority = "High",
                    ProjectId = 1
                });
            }

            if (!db.TaskAssignments.Any())
            {
                db.TaskAssignments.Add(new TaskAssignment
                {
                    UserId = 1,
                    TaskItemId = 1
                });
            }

            db.SaveChanges();
        });
    }
}
