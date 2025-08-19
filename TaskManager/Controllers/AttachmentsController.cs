using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public AttachmentsController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAttachments(int taskId)
        {
            var attachments = await _context.Attachments
                .Where(a => a.TaskItemId == taskId)
                .ToListAsync();
            return Ok(attachments);
        }

        [HttpPost]
        public async Task<IActionResult> AddAttachment(int taskId, Attachment attachment)
        {
            attachment.TaskItemId = taskId;
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAttachments), new { taskId = taskId }, attachment);
        }
    }
}


