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

// Add services to the container.
builder.Services.AddDbContext<TaskDbContext>(options =>
{
    var useInMemoryDatabase = builder.Environment.IsEnvironment("Testing")
        || builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

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
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section.");

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

    var useInMemoryDatabase = app.Environment.IsEnvironment("Testing")
        || app.Configuration.GetValue<bool>("UseInMemoryDatabase");

    if (useInMemoryDatabase)
    {
        dbContext.Database.EnsureCreated();
    }

    if (!await dbContext.Users.AnyAsync(u => u.Email == "admin@taskmanager.local"))
    {
        passwordHasher.CreatePasswordHash("Admin@123", out var adminHash, out var adminSalt);
        dbContext.Users.Add(new User
        {
            Name = "System Admin",
            Email = "admin@taskmanager.local",
            Role = AppRoles.Admin,
            PasswordHash = adminHash,
            PasswordSalt = adminSalt
        });
    }

    if (!await dbContext.Users.AnyAsync(u => u.Email == "manager@taskmanager.local"))
    {
        passwordHasher.CreatePasswordHash("Manager@123", out var managerHash, out var managerSalt);
        dbContext.Users.Add(new User
        {
            Name = "Project Manager",
            Email = "manager@taskmanager.local",
            Role = AppRoles.Manager,
            PasswordHash = managerHash,
            PasswordSalt = managerSalt
        });
    }

    await dbContext.SaveChangesAsync();
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

public partial class Program { }
