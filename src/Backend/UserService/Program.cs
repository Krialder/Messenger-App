// ========================================
// PSEUDO-CODE - Sprint 7: User Service Program
// ========================================

using Microsoft.EntityFrameworkCore;
using Serilog;
using UserService.Data;
using MessengerCommon.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Database
var connectionString = builder.Configuration.GetConnectionString("UserDatabase");
builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// JWT Authentication (using extension method)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!);

// Swagger (using extension method)
builder.Services.AddSwaggerWithJwt("UserService API", "v1");

// CORS (using extension method)
builder.Services.AddDefaultCors(builder.Configuration);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting UserService...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "UserService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
