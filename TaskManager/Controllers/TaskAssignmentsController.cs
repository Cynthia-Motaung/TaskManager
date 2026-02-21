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
    public class TaskAssignmentsController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public TaskAssignmentsController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetTaskAssignments()
        {
            var assignments = await _context.TaskAssignments
                .AsNoTracking()
                .Include(ta => ta.User)
                .Include(ta => ta.TaskItem)
                .ToListAsync();

            return Ok(assignments.Select(ta => ta.ToTaskAssignmentDto()));
        }

        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetTaskAssignmentsByTask(int taskId)
        {
            var assignments = await _context.TaskAssignments
                .AsNoTracking()
                .Include(ta => ta.User)
                .Include(ta => ta.TaskItem)
                .Where(ta => ta.TaskItemId == taskId)
                .ToListAsync();

            return Ok(assignments.Select(ta => ta.ToTaskAssignmentDto()));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTaskAssignmentsByUser(int userId)
        {
            var assignments = await _context.TaskAssignments
                .AsNoTracking()
                .Include(ta => ta.User)
                .Include(ta => ta.TaskItem)
                .Where(ta => ta.UserId == userId)
                .ToListAsync();

            return Ok(assignments.Select(ta => ta.ToTaskAssignmentDto()));
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
        [HttpPost]
        public async Task<IActionResult> CreateTaskAssignment(TaskAssignmentCreateDto taskAssignmentDto)
        {
            // Verify task and user exist
            var task = await _context.Tasks.FindAsync(taskAssignmentDto.TaskItemId);
            var user = await _context.Users.FindAsync(taskAssignmentDto.UserId);

            if (task == null || user == null)
                return BadRequest("Invalid task or user ID.");

            // Check if assignment already exists
            var existingAssignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(ta => ta.TaskItemId == taskAssignmentDto.TaskItemId && ta.UserId == taskAssignmentDto.UserId);

            if (existingAssignment != null)
                return BadRequest("This assignment already exists.");

            var taskAssignment = new TaskAssignment
            {
                TaskItemId = taskAssignmentDto.TaskItemId,
                UserId = taskAssignmentDto.UserId
            };

            _context.TaskAssignments.Add(taskAssignment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskAssignments), taskAssignment.ToTaskAssignmentDto());
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
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
