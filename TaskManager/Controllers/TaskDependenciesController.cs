using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskDependenciesController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public TaskDependenciesController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetTaskDependencies() =>
            Ok(await _context.TaskDependencies
                .Include(td => td.TaskItem)
                .Include(td => td.DependsOnTask)
                .ToListAsync());

        [HttpGet("{taskId}/{dependsOnTaskId}")]
        public async Task<IActionResult> GetTaskDependency(int taskId, int dependsOnTaskId)
        {
            var dependency = await _context.TaskDependencies
                .Include(td => td.TaskItem)
                .Include(td => td.DependsOnTask)
                .FirstOrDefaultAsync(td => td.TaskItemId == taskId && td.DependsOnTaskId == dependsOnTaskId);

            return dependency == null ? NotFound() : Ok(dependency);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskDependency(TaskDependency taskDependency)
        {
            // Verify both tasks exist
            var taskItem = await _context.Tasks.FindAsync(taskDependency.TaskItemId);
            var dependsOnTask = await _context.Tasks.FindAsync(taskDependency.DependsOnTaskId);
            
            if (taskItem == null || dependsOnTask == null)
                return BadRequest("One or both task IDs are invalid.");

            if (taskDependency.TaskItemId == taskDependency.DependsOnTaskId)
                return BadRequest("A task cannot depend on itself.");

            _context.TaskDependencies.Add(taskDependency);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskDependency), 
                new { taskId = taskDependency.TaskItemId, dependsOnTaskId = taskDependency.DependsOnTaskId }, 
                taskDependency);
        }

        [HttpDelete("{taskId}/{dependsOnTaskId}")]
        public async Task<IActionResult> DeleteTaskDependency(int taskId, int dependsOnTaskId)
        {
            var dependency = await _context.TaskDependencies
                .FirstOrDefaultAsync(td => td.TaskItemId == taskId && td.DependsOnTaskId == dependsOnTaskId);
            
            if (dependency == null) return NotFound();
            
            _context.TaskDependencies.Remove(dependency);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
