using expense_tracker.Data;
using expense_tracker.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExpenseTrackerDatabase")));

builder.Services.AddScoped<IExpenseService, ExpenseService>();

var app = builder.Build();

app.Logger.LogTrace("Trace log: Detailed diagnostic information.");

app.Logger.LogDebug("Debug log: Variable values and debugging information.");

app.Logger.LogInformation("Application started at {Time}", DateTime.UtcNow);

app.Logger.LogWarning("Storage space is running low. Remaining: {Space} MB", 500);

app.Logger.LogError("Failed to connect to the database.");

app.Logger.LogCritical("Application is shutting down due to a critical failure.");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
