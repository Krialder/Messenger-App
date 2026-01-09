using KeyManagementService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyManagementService.BackgroundServices;

/// <summary>
/// Background service that automatically rotates expired keys every 24 hours.
/// </summary>
public class KeyRotationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KeyRotationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public KeyRotationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<KeyRotationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Key Rotation Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RotateExpiredKeysAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rotating expired keys.");
            }

            // Wait 24 hours before next check
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Key Rotation Background Service stopped.");
    }

    private async Task RotateExpiredKeysAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting automatic key rotation check...");

        using IServiceScope scope = _serviceProvider.CreateScope();
        IKeyRotationService keyRotationService = scope.ServiceProvider.GetRequiredService<IKeyRotationService>();

        await keyRotationService.RotateExpiredKeysAsync(cancellationToken);

        _logger.LogInformation("Automatic key rotation check completed.");
    }
}
