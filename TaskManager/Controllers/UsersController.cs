using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.DTOs;
using TaskManager.Mappings;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly TaskDbContext _context;
        public UsersController(TaskDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            return Ok(users.Select(u => u.ToUserDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.TaskAssignments)
                .ThenInclude(ta => ta.TaskItem)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? NotFound() : Ok(user.ToUserDetailsDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserCreateDto userDto)
        {
            var emailInUse = await _context.Users.AnyAsync(u => u.Email == userDto.Email);
            if (emailInUse)
            {
                ModelState.AddModelError(nameof(userDto.Email), "Email is already in use.");
                return ValidationProblem(ModelState);
            }

            var user = new User
            {
                Name = userDto.Name.Trim(),
                Email = userDto.Email.Trim()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user.ToUserDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userDto)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null) return NotFound();

            var emailInUse = await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id);
            if (emailInUse)
            {
                ModelState.AddModelError(nameof(userDto.Email), "Email is already in use.");
                return ValidationProblem(ModelState);
            }

            existingUser.Name = userDto.Name.Trim();
            existingUser.Email = userDto.Email.Trim();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
