using CryptoService.Layer1;
using CryptoService.Layer2;
using MessengerContracts.Interfaces;
using MessengerCommon.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();

// JWT Authentication (using extension method)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Crypto Services
builder.Services.AddScoped<ITransportEncryptionService, TransportEncryptionService>();
builder.Services.AddScoped<ILocalStorageEncryptionService, LocalStorageEncryptionService>();

// Swagger (using extension method)
builder.Services.AddSwaggerWithJwt("Crypto API", "v1");

// CORS (using extension method)
builder.Services.AddDefaultCors(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks();

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

Log.Information("CryptoService started successfully");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
