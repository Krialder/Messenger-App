using AuthService.Data;
using AuthService.Services;
using MessengerContracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using MessengerCommon.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Database
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Authentication (using extension method)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Validate TOTP Encryption Key
var totpEncryptionKey = Environment.GetEnvironmentVariable("TOTP_ENCRYPTION_KEY") 
    ?? builder.Configuration["Security:TotpEncryptionKey"];

if (string.IsNullOrEmpty(totpEncryptionKey))
{
    Log.Warning("TOTP_ENCRYPTION_KEY not configured. Using development fallback (NOT FOR PRODUCTION!)");
}
else if (totpEncryptionKey.Length < 32)
{
    throw new InvalidOperationException(
        "TOTP_ENCRYPTION_KEY must be at least 32 characters. " +
        "Set TOTP_ENCRYPTION_KEY environment variable with a secure random key. " +
        "Generate with: openssl rand -base64 64");
}
else
{
    Log.Information("TOTP encryption key configured successfully");
}

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

builder.Services.Configure<IpRateLimitPolicies>(options =>
{
    options.IpRules = new List<IpRateLimitPolicy>();
});

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Services
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMfaService, MFAService>();

// Swagger (using extension method)
builder.Services.AddSwaggerWithJwt("Auth API", "v1");

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

// Rate limiting middleware (must be early in pipeline)
app.UseIpRateLimiting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
