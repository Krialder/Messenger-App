using AuditLogService.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MessengerCommon.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/audit-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();

// Database
var connectionString = builder.Configuration.GetConnectionString("AuditDatabase") 
    ?? throw new InvalidOperationException("AuditDatabase connection string not configured");
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Authentication (using extension method)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Swagger (using extension method)
builder.Services.AddSwaggerWithJwt("Audit Log API", "v1");

// CORS (using extension method)
builder.Services.AddDefaultCors(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("AuditLogService started successfully");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
