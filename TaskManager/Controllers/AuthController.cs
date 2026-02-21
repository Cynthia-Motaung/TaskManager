using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.DTOs;
using TaskManager.Mappings;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TaskDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(TaskDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRegisterDto registerDto)
    {
        var normalizedEmail = registerDto.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(registerDto.Email), "Email is already in use.");
            return ValidationProblem(ModelState);
        }

        _passwordHasher.CreatePasswordHash(registerDto.Password, out var hash, out var salt);

        var user = new User
        {
            Name = registerDto.Name.Trim(),
            Email = normalizedEmail,
            Role = AppRoles.User,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (token, expiresAtUtc) = _tokenService.CreateToken(user);
        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc
        };

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthLoginDto loginDto)
    {
        var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null || user.PasswordHash == null || user.PasswordSalt == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var validPassword = _passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt);
        if (!validPassword)
        {
            return Unauthorized("Invalid email or password.");
        }

        var (token, expiresAtUtc) = _tokenService.CreateToken(user);
        var response = new AuthResponseDto
        {
            UserId = user.Id,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? NotFound() : Ok(user.ToUserDto());
    }
}
