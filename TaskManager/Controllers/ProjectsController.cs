using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.DTOs;
using TaskManager.Mappings;
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
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _context.Projects
                .AsNoTracking()
                .ToListAsync();

            return Ok(projects.Select(p => p.ToProjectDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            return project == null ? NotFound() : Ok(project.ToProjectDetailsDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectCreateDto projectDto)
        {
            var project = new Project
            {
                Name = projectDto.Name.Trim(),
                Description = projectDto.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project.ToProjectDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto projectDto)
        {
            var existingProject = await _context.Projects.FindAsync(id);
            if (existingProject == null) return NotFound();

            existingProject.Name = projectDto.Name.Trim();
            existingProject.Description = projectDto.Description?.Trim();
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

