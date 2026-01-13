using AuthService.Data;
using AuthService.Services;
using MessengerContracts.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. CONTROLLERS & API CONFIGURATION
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================
// 2. DATABASE - PostgreSQL with EF Core
// ============================================
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// 3. JWT AUTHENTICATION
// ============================================
var jwtSettings = builder.Configuration.GetSection("JWT");

// Security: Validate JWT secret strength (minimum 256 bits)
var jwtSecret = jwtSettings["Secret"];
if (string.IsNullOrEmpty(jwtSecret) || Encoding.UTF8.GetBytes(jwtSecret).Length < 32)
{
    throw new InvalidOperationException(
        "JWT Secret must be at least 32 characters (256 bits). " +
        "Generate with: openssl rand -base64 64");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero  // No tolerance for expired tokens
        };
    });

// ============================================
// 4. CORS - Cross-Origin Resource Sharing
// ============================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Frontend URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================
// 5. DEPENDENCY INJECTION - Application Services
// ============================================
builder.Services.AddScoped<IMfaService, MFAService>();
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ============================================
// 6. HEALTH CHECKS
// ============================================
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE CONFIGURATION
// ============================================

// Development-only: Swagger UI for API documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enforce HTTPS for production security
app.UseHttpsRedirection();

// Enable CORS (must be before Authentication)
app.UseCors();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers();

// Health check endpoint for monitoring
app.MapHealthChecks("/health");

app.Run();
