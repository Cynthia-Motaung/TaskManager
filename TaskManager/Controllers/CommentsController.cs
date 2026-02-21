using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.DTOs;
using TaskManager.Mappings;
using TaskManager.Models;


 namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public CommentsController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetComments(int taskId)
        {
            var comments = await _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.TaskItemId == taskId)
                .ToListAsync();
            return Ok(comments.Select(c => c.ToCommentDto()));
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int taskId, CommentCreateDto commentDto)
        {
            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
            if (!taskExists) return NotFound("Task not found.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == commentDto.UserId);
            if (!userExists)
            {
                ModelState.AddModelError(nameof(commentDto.UserId), "User does not exist.");
                return ValidationProblem(ModelState);
            }

            var comment = new Comment
            {
                TaskItemId = taskId,
                UserId = commentDto.UserId,
                Content = commentDto.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .FirstAsync(c => c.Id == comment.Id);

            return CreatedAtAction(nameof(GetComments), new { taskId = taskId }, createdComment.ToCommentDto());
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int taskId, int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.TaskItemId == taskId);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
