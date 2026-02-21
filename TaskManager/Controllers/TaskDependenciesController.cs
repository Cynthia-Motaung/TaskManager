using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TaskManager.DTOs;
using TaskManager.Mappings;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TaskDependenciesController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public TaskDependenciesController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetTaskDependencies()
        {
            var dependencies = await _context.TaskDependencies
                .AsNoTracking()
                .Include(td => td.TaskItem)
                .Include(td => td.DependsOnTask)
                .ToListAsync();

            return Ok(dependencies.Select(td => td.ToTaskDependencyDto()));
        }

        [HttpGet("{taskId}/{dependsOnTaskId}")]
        public async Task<IActionResult> GetTaskDependency(int taskId, int dependsOnTaskId)
        {
            var dependency = await _context.TaskDependencies
                .AsNoTracking()
                .Include(td => td.TaskItem)
                .Include(td => td.DependsOnTask)
                .FirstOrDefaultAsync(td => td.TaskItemId == taskId && td.DependsOnTaskId == dependsOnTaskId);

            return dependency == null ? NotFound() : Ok(dependency.ToTaskDependencyDto());
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
        [HttpPost]
        public async Task<IActionResult> CreateTaskDependency(TaskDependencyCreateDto taskDependencyDto)
        {
            // Verify both tasks exist
            var taskItem = await _context.Tasks.FindAsync(taskDependencyDto.TaskItemId);
            var dependsOnTask = await _context.Tasks.FindAsync(taskDependencyDto.DependsOnTaskId);
            
            if (taskItem == null || dependsOnTask == null)
                return BadRequest("One or both task IDs are invalid.");

            if (taskDependencyDto.TaskItemId == taskDependencyDto.DependsOnTaskId)
                return BadRequest("A task cannot depend on itself.");

            var dependencyExists = await _context.TaskDependencies
                .AnyAsync(td =>
                    td.TaskItemId == taskDependencyDto.TaskItemId &&
                    td.DependsOnTaskId == taskDependencyDto.DependsOnTaskId);

            if (dependencyExists)
                return BadRequest("This dependency already exists.");

            var taskDependency = new TaskDependency
            {
                TaskItemId = taskDependencyDto.TaskItemId,
                DependsOnTaskId = taskDependencyDto.DependsOnTaskId
            };

            _context.TaskDependencies.Add(taskDependency);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskDependency),
                new { taskId = taskDependency.TaskItemId, dependsOnTaskId = taskDependency.DependsOnTaskId }, 
                taskDependency.ToTaskDependencyDto());
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
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
