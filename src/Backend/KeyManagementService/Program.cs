// ========================================
// PSEUDO-CODE - Sprint 6: Key Management Service Program
// Status: 🔶 Bootstrap-Konfiguration
// ========================================

using SecureMessenger.KeyManagementService.Data;
using SecureMessenger.KeyManagementService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// PSEUDO: Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PSEUDO: Database Context
builder.Services.AddDbContext<KeyDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("KeyDatabase");
    options.UseNpgsql(connectionString);
});

// PSEUDO: Background Services
builder.Services.AddHostedService<KeyRotationService>();

// PSEUDO: Business Services
// builder.Services.AddScoped<IKeyManagementService, KeyManagementServiceImpl>();

// PSEUDO: Authentication
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { ... });

// PSEUDO: Logging
// builder.Services.AddSerilog();

var app = builder.Build();

// PSEUDO: Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
