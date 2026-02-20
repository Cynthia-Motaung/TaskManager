using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskAssignmentsController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public TaskAssignmentsController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetTaskAssignments() =>
            Ok(await _context.TaskAssignments
                .Include(ta => ta.User)
                .Include(ta => ta.TaskItem)
                .ToListAsync());

        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetTaskAssignmentsByTask(int taskId)
        {
            var assignments = await _context.TaskAssignments
                .Include(ta => ta.User)
                .Where(ta => ta.TaskItemId == taskId)
                .ToListAsync();

            return Ok(assignments);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTaskAssignmentsByUser(int userId)
        {
            var assignments = await _context.TaskAssignments
                .Include(ta => ta.TaskItem)
                .Where(ta => ta.UserId == userId)
                .ToListAsync();

            return Ok(assignments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaskAssignment(TaskAssignment taskAssignment)
        {
            // Verify task and user exist
            var task = await _context.Tasks.FindAsync(taskAssignment.TaskItemId);
            var user = await _context.Users.FindAsync(taskAssignment.UserId);

            if (task == null || user == null)
                return BadRequest("Invalid task or user ID.");

            // Check if assignment already exists
            var existingAssignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(ta => ta.TaskItemId == taskAssignment.TaskItemId && ta.UserId == taskAssignment.UserId);

            if (existingAssignment != null)
                return BadRequest("This assignment already exists.");

            _context.TaskAssignments.Add(taskAssignment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskAssignments), taskAssignment);
        }

        [HttpDelete("{userId}/{taskId}")]
        public async Task<IActionResult> DeleteTaskAssignment(int userId, int taskId)
        {
            var assignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(ta => ta.UserId == userId && ta.TaskItemId == taskId);

            if (assignment == null) return NotFound();

            _context.TaskAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
