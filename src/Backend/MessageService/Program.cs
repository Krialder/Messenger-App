// PSEUDO CODE: Message Service Startup Configuration

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
// builder.Services.AddDbContext<MessageDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// SignalR
builder.Services.AddSignalR();

// Redis (for presence management)
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration["Redis:ConnectionString"];
// });

// RabbitMQ
// builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

// JWT Authentication
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         // Same JWT configuration as Auth Service
//     });

// CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
// app.MapHub<NotificationHub>("/hubs/notifications");

// Start RabbitMQ consumer
// var rabbitMQ = app.Services.GetRequiredService<IRabbitMQService>();
// rabbitMQ.StartConsuming();

app.Run();
