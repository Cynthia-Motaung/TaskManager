using Microsoft.EntityFrameworkCore;

namespace TaskManager.Models
{
    public class TaskDbContext :DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options):base(options)
        {
        }
        
    }
}
