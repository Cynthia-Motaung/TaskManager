using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Configuration;
using TaskManager.DTOs;
using TaskManager.Middleware;
using TaskManager.Models;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");
var useInMemoryDatabase = isTesting || builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

// Add services to the container.
builder.Services.AddDbContext<TaskDbContext>(options =>
{
    if (useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("TaskManagerLocalDb");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("TaskConnection"));
    }
});
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section.");

var jwtKeyFromEnv = Environment.GetEnvironmentVariable("TASKMANAGER_JWT_KEY");
if (!string.IsNullOrWhiteSpace(jwtKeyFromEnv))
{
    jwtSettings.Key = jwtKeyFromEnv;
}

if (isTesting && IsMissingOrPlaceholder(jwtSettings.Key))
{
    jwtSettings.Key = "TaskManager-Testing-Secret-Key-012345678901234567890123456";
}

if (IsMissingOrPlaceholder(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "JWT key is not configured. Set TASKMANAGER_JWT_KEY (recommended) or Jwt:Key via secret configuration.");
}

if (jwtSettings.Key.Length < 32)
{
    throw new InvalidOperationException("JWT key must be at least 32 characters.");
}

builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
builder.Services.AddAuthorization();

//Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter token as: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add OpenAPI support
builder.Services.AddOpenApi();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    if (useInMemoryDatabase)
    {
        dbContext.Database.EnsureCreated();
    }

    var seedUsersEnabled = isTesting || app.Configuration.GetValue<bool>("SeedUsers:Enabled");
    if (seedUsersEnabled)
    {
        var adminEmail = (app.Configuration["SeedUsers:AdminEmail"] ?? "admin@taskmanager.local")
            .Trim()
            .ToLowerInvariant();
        var managerEmail = (app.Configuration["SeedUsers:ManagerEmail"] ?? "manager@taskmanager.local")
            .Trim()
            .ToLowerInvariant();

        var adminPassword = app.Configuration["SeedUsers:AdminPassword"];
        var managerPassword = app.Configuration["SeedUsers:ManagerPassword"];

        if (isTesting)
        {
            adminPassword ??= "Admin@123";
            managerPassword ??= "Manager@123";
        }
        else
        {
            EnsureStrongSeedPassword(adminPassword, "SeedUsers:AdminPassword");
            EnsureStrongSeedPassword(managerPassword, "SeedUsers:ManagerPassword");
        }

        await UpsertSeedUserAsync(adminEmail, "System Admin", AppRoles.Admin, adminPassword!);
        await UpsertSeedUserAsync(managerEmail, "Project Manager", AppRoles.Manager, managerPassword!);

        await dbContext.SaveChangesAsync();
    }

    async Task UpsertSeedUserAsync(string email, string name, string role, string password)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        passwordHasher.CreatePasswordHash(password, out var hash, out var salt);

        if (user == null)
        {
            dbContext.Users.Add(new User
            {
                Name = name,
                Email = email,
                Role = role,
                PasswordHash = hash,
                PasswordSalt = salt
            });
            return;
        }

        user.Name = name;
        user.Role = role;
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();
app.Run();

static bool IsMissingOrPlaceholder(string? value) =>
    string.IsNullOrWhiteSpace(value) || value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);

static void EnsureStrongSeedPassword(string? value, string settingName)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"{settingName} is required when SeedUsers:Enabled is true.");
    }

    if (value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"{settingName} must be changed from placeholder value.");
    }

    if (value.Length < 12)
    {
        throw new InvalidOperationException($"{settingName} must be at least 12 characters.");
    }
}

public partial class Program { }
