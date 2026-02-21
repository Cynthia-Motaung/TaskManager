using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.DTOs;
using TaskManager.Mappings;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
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
                .AsNoTracking()
                .Where(a => a.TaskItemId == taskId)
                .ToListAsync();
            return Ok(attachments.Select(a => a.ToAttachmentDto()));
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
        [HttpPost]
        public async Task<IActionResult> AddAttachment(int taskId, AttachmentCreateDto attachmentDto)
        {
            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
            if (!taskExists) return NotFound("Task not found.");

            var attachment = new Attachment
            {
                TaskItemId = taskId,
                FileName = attachmentDto.FileName.Trim(),
                FileUrl = attachmentDto.FileUrl.Trim(),
                UploadedAt = DateTime.UtcNow
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAttachments), new { taskId = taskId }, attachment.ToAttachmentDto());
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
        [HttpDelete("{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int taskId, int attachmentId)
        {
            var attachment = await _context.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskItemId == taskId);
            if (attachment == null) return NotFound();

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
