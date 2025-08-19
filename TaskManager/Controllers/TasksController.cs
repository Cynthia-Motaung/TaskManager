using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        public async Task<IActionResult> GetTasks() =>
            Ok(await _context.Tasks
                .Include(t => t.TaskAssignments)
                .Include(t => t.Dependencies)
                .ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskAssignments)
                .Include(t => t.Dependencies)
                .FirstOrDefaultAsync(t => t.Id == id);

            return task == null ? NotFound() : Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            if (id != task.Id) return BadRequest();
            _context.Entry(task).State = EntityState.Modified;
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

            _context.TaskAssignments.Add(new TaskAssignment { TaskItemId = id, UserId = userId });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterTasks(
        [FromQuery] string status,
        [FromQuery] string priority,
        [FromQuery] int? projectId,
        [FromQuery] int? userId)
        {
            var query = _context.Tasks
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId);

            if (userId.HasValue)
                query = query.Where(t => t.TaskAssignments.Any(ta => ta.UserId == userId));

            var tasks = await query.ToListAsync();
            return Ok(tasks);
        }

    }

}

