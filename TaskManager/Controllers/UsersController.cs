using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.DTOs;
using TaskManager.Mappings;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly TaskDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UsersController(TaskDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

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

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserCreateDto userDto)
        {
            if (!AppRoles.ValidRoles.Contains(userDto.Role))
            {
                ModelState.AddModelError(nameof(userDto.Role), "Role must be User, Manager, or Admin.");
                return ValidationProblem(ModelState);
            }

            var normalizedEmail = userDto.Email.Trim().ToLowerInvariant();
            var emailInUse = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (emailInUse)
            {
                ModelState.AddModelError(nameof(userDto.Email), "Email is already in use.");
                return ValidationProblem(ModelState);
            }

            byte[]? hash = null;
            byte[]? salt = null;
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                _passwordHasher.CreatePasswordHash(userDto.Password, out var createdHash, out var createdSalt);
                hash = createdHash;
                salt = createdSalt;
            }

            var user = new User
            {
                Name = userDto.Name.Trim(),
                Email = normalizedEmail,
                Role = NormalizeRole(userDto.Role),
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user.ToUserDto());
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userDto)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null) return NotFound();

            if (!AppRoles.ValidRoles.Contains(userDto.Role))
            {
                ModelState.AddModelError(nameof(userDto.Role), "Role must be User, Manager, or Admin.");
                return ValidationProblem(ModelState);
            }

            var normalizedEmail = userDto.Email.Trim().ToLowerInvariant();
            var emailInUse = await _context.Users.AnyAsync(u => u.Email == normalizedEmail && u.Id != id);
            if (emailInUse)
            {
                ModelState.AddModelError(nameof(userDto.Email), "Email is already in use.");
                return ValidationProblem(ModelState);
            }

            existingUser.Name = userDto.Name.Trim();
            existingUser.Email = normalizedEmail;
            existingUser.Role = NormalizeRole(userDto.Role);

            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                _passwordHasher.CreatePasswordHash(userDto.Password, out var hash, out var salt);
                existingUser.PasswordHash = hash;
                existingUser.PasswordSalt = salt;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static string NormalizeRole(string role) =>
            role.Trim().ToLowerInvariant() switch
            {
                "admin" => AppRoles.Admin,
                "manager" => AppRoles.Manager,
                _ => AppRoles.User
            };
    }

}
