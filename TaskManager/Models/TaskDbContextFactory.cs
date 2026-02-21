using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManager.Models;

public class TaskDbContextFactory : IDesignTimeDbContextFactory<TaskDbContext>
{
    public TaskDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TaskDbContext>();

        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__TaskConnection")
            ?? Environment.GetEnvironmentVariable("TASKMANAGER_CONNECTIONSTRING");

        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Connection string is not configured for EF design-time operations. Set ConnectionStrings__TaskConnection (recommended) or TASKMANAGER_CONNECTIONSTRING.");
        }

        optionsBuilder.UseSqlServer(connectionString);
        return new TaskDbContext(optionsBuilder.Options);
    }
}
