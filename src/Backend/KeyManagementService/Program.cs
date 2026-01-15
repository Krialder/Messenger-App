// ========================================
// Key Management Service Program
// Status: 🚀 Production
// ========================================

using KeyManagementService.BackgroundServices;
using KeyManagementService.Data;
using KeyManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
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

// Add Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Database
var connectionString = builder.Configuration.GetConnectionString("KeyDatabase");
builder.Services.AddDbContext<KeyDbContext>(options =>
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

// Services
builder.Services.AddScoped<IKeyRotationService, KeyRotationService>();

// Background Services
builder.Services.AddHostedService<KeyRotationBackgroundService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!);

// Swagger (using extension method)
builder.Services.AddSwaggerWithJwt("KeyManagementService API", "v1");

// CORS (using extension method)
builder.Services.AddDefaultCors(builder.Configuration);

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KeyManagementService API v1");
    });
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
    Log.Information("Starting KeyManagementService...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "KeyManagementService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
