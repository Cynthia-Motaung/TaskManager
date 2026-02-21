using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManager.Models;

public class TaskDbContextFactory : IDesignTimeDbContextFactory<TaskDbContext>
{
    public TaskDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TaskDbContext>();

        var connectionString =
            Environment.GetEnvironmentVariable("TASKMANAGER_CONNECTIONSTRING")
            ?? "Server=localhost,14333;Database=TaskManager;User ID=sa;Password=TaskManager!234;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True;";

        optionsBuilder.UseSqlServer(connectionString);
        return new TaskDbContext(optionsBuilder.Options);
    }
}
