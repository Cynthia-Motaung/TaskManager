using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .Include(c => c.User)
                .Where(c => c.TaskItemId == taskId)
                .ToListAsync();
            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int taskId, Comment comment)
        {
            comment.TaskItemId = taskId;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetComments), new { taskId = taskId }, comment);
        }
    }
}

