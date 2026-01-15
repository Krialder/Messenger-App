using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MessageService.Data;
using MessageService.Hubs;
using MessageService.Services;
using AspNetCoreRateLimit;
using MessengerCommon.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. Controllers & JSON Configuration
// ========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ========================================
// 2. Database - EF Core + PostgreSQL
// ========================================
builder.Services.AddDbContext<MessageDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MessageDatabase");
    options.UseNpgsql(connectionString);
    
    // Enable sensitive data logging in development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ========================================
// 3. SignalR for Real-time Communication
// ========================================
builder.Services.AddSignalR();

// ========================================
// 4. Rate Limiting
// ========================================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/messages",
            Limit = 60,
            Period = "1m"
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/groups",
            Limit = 10,
            Period = "1h"
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/groups/*/members",
            Limit = 30,
            Period = "1h"
        }
    };
});

builder.Services.Configure<IpRateLimitPolicies>(options =>
{
    options.IpRules = new List<IpRateLimitPolicy>();
});

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ========================================
// 5. CORS (using extension method)
// ========================================
builder.Services.AddDefaultCors(builder.Configuration);

// ========================================
// 6. JWT Authentication (using extension method with SignalR support)
// ========================================
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configure SignalR JWT from query string
builder.Services.ConfigureAll<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(options =>
{
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            // If request is for SignalR hub and token is present
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

// ========================================
// 7. Swagger/OpenAPI (using extension method)
// ========================================
builder.Services.AddSwaggerWithJwt("MessageService API", "v1");

// ========================================
// 8. Health Checks
// ========================================
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("MessageDatabase")!);

// ========================================
// 9. Logging
// ========================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}

// ========================================
// RabbitMQ Service
// ========================================
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

// ========================================
// Build Application
// ========================================
var app = builder.Build();

// ========================================
// Middleware Pipeline
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessageService API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Rate limiting middleware (must be early in pipeline)
app.UseIpRateLimiting();

// CORS must be before Authentication/Authorization
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

// Health check endpoint
app.MapHealthChecks("/health");

// ========================================
// Run Application
// ========================================
app.Run();
