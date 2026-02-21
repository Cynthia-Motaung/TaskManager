using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TaskManager.DTOs;
using TaskManager.Mappings;
using TaskManager.Models;


namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public TasksController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Include(t => t.Dependencies)
                .ToListAsync();

            return Ok(tasks.Select(t => t.ToTaskDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Include(t => t.Dependencies)
                .FirstOrDefaultAsync(t => t.Id == id);

            return task == null ? NotFound() : Ok(task.ToTaskDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateDto taskDto)
        {
            ValidateTaskFields(taskDto, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == taskDto.ProjectId);
            if (!projectExists)
            {
                ModelState.AddModelError(nameof(taskDto.ProjectId), "Project does not exist.");
                return ValidationProblem(ModelState);
            }

            var task = new TaskItem
            {
                Title = taskDto.Title.Trim(),
                Description = taskDto.Description?.Trim(),
                Status = NormalizeStatus(taskDto.Status),
                Priority = NormalizePriority(taskDto.Priority),
                DueDate = taskDto.DueDate,
                ProjectId = taskDto.ProjectId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task.ToTaskDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskUpdateDto taskDto)
        {
            ValidateTaskFields(taskDto, ModelState);
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == taskDto.ProjectId);
            if (!projectExists)
            {
                ModelState.AddModelError(nameof(taskDto.ProjectId), "Project does not exist.");
                return ValidationProblem(ModelState);
            }

            task.Title = taskDto.Title.Trim();
            task.Description = taskDto.Description?.Trim();
            task.Status = NormalizeStatus(taskDto.Status);
            task.Priority = NormalizePriority(taskDto.Priority);
            task.DueDate = taskDto.DueDate;
            task.ProjectId = taskDto.ProjectId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/assign/{userId}")]
        public async Task<IActionResult> AssignUser(int id, int userId)
        {
            var task = await _context.Tasks.FindAsync(id);
            var user = await _context.Users.FindAsync(userId);
            if (task == null || user == null) return NotFound();

            var assignmentExists = await _context.TaskAssignments
                .AnyAsync(a => a.TaskItemId == id && a.UserId == userId);
            if (assignmentExists)
            {
                ModelState.AddModelError("assignment", "This user is already assigned to the task.");
                return ValidationProblem(ModelState);
            }

            _context.TaskAssignments.Add(new TaskAssignment { TaskItemId = id, UserId = userId });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterTasks(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] int? projectId,
        [FromQuery] int? userId)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Include(t => t.Dependencies)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId);

            if (userId.HasValue)
                query = query.Where(t => _context.TaskAssignments
                    .Any(ta => ta.TaskItemId == t.Id && ta.UserId == userId.Value));

            var tasks = await query.ToListAsync();
            return Ok(tasks.Select(t => t.ToTaskDto()));
        }

        private static void ValidateTaskFields(TaskCreateDto taskDto, ModelStateDictionary modelState)
        {
            if (!TaskFieldRules.ValidStatuses.Contains(taskDto.Status))
            {
                modelState.AddModelError(nameof(taskDto.Status),
                    "Status must be one of: Pending, InProgress, Done, Blocked.");
            }

            if (!TaskFieldRules.ValidPriorities.Contains(taskDto.Priority))
            {
                modelState.AddModelError(nameof(taskDto.Priority),
                    "Priority must be one of: Low, Medium, High, Critical.");
            }
        }

        private static string NormalizeStatus(string status) =>
            status.Trim().ToLowerInvariant() switch
            {
                "pending" => "Pending",
                "inprogress" => "InProgress",
                "done" => "Done",
                "blocked" => "Blocked",
                _ => status
            };

        private static string NormalizePriority(string priority) =>
            priority.Trim().ToLowerInvariant() switch
            {
                "low" => "Low",
                "medium" => "Medium",
                "high" => "High",
                "critical" => "Critical",
                _ => priority
            };

    }

}
