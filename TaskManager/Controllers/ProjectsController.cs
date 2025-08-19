using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public ProjectsController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetProjects() =>
            Ok(await _context.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.TaskAssignments)
                .ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.TaskAssignments)
                .FirstOrDefaultAsync(p => p.Id == id);

            return project == null ? NotFound() : Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, Project project)
        {
            if (id != project.Id) return BadRequest();
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


