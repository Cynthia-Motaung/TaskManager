using Microsoft.EntityFrameworkCore;
using TaskManager.Middleware;
using TaskManager.Models;

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

//Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenAPI support
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();
app.Run();

public partial class Program { }
