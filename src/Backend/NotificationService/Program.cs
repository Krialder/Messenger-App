using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Hubs;
using NotificationService.Services;
using Serilog;
using System.Text;
using MessengerCommon.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

// JWT Authentication (using extension method)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configure JWT for SignalR
builder.Services.ConfigureAll<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(options =>
{
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// SignalR
builder.Services.AddSignalR();

// RabbitMQ Consumer Background Service
builder.Services.AddHostedService<RabbitMQConsumerService>();

// CORS (using extension method)
builder.Services.AddDefaultCors(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

// Middleware
app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

// Health checks
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting NotificationService...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NotificationService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
