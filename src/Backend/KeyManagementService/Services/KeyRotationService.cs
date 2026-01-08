// ========================================
// PSEUDO-CODE - Sprint 6: Key Rotation Service
// Status: 🔶 Interface definiert
// Background Service: Automatische Schlüsselrotation
// ========================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SecureMessenger.KeyManagementService.Services;

/// <summary>
/// Background service for automatic key rotation and expiration management
/// </summary>
public class KeyRotationService : BackgroundService
{
    // TODO-SPRINT-6: Inject IServiceProvider, ILogger
    
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // PSEUDO: Background task runs every hour
        while (!stoppingToken.IsCancellationRequested)
        {
            // PSEUDO: Check for expired keys
            await CheckExpiredKeysAsync(stoppingToken);
            
            // PSEUDO: Check for keys nearing expiration (send notifications)
            await NotifyExpiringKeysAsync(stoppingToken);
            
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
    
    private async Task CheckExpiredKeysAsync(CancellationToken cancellationToken)
    {
        // PSEUDO: Load all keys where expires_at < NOW() AND status = 'active'
        // PSEUDO: Mark keys as expired
        // PSEUDO: Log expiration events
        // PSEUDO: Send notification to users (optional)
        
        // INTERFACE: Database query
        // var expiredKeys = await _dbContext.PublicKeys
        //     .Where(k => k.ExpiresAt < DateTime.UtcNow && k.Status == "active")
        //     .ToListAsync(cancellationToken);
    }
    
    private async Task NotifyExpiringKeysAsync(CancellationToken cancellationToken)
    {
        // PSEUDO: Load keys expiring within 7 days
        // PSEUDO: Send notification to users
        // PSEUDO: Suggest key rotation
        
        var expirationThreshold = DateTime.UtcNow.AddDays(7);
        
        // INTERFACE: Notification service
        // await _notificationService.SendKeyExpirationWarningAsync(userId, keyId);
    }
}

/// <summary>
/// Service for key lifecycle management
/// </summary>
public interface IKeyManagementService
{
    // INTERFACE: Get public key by user ID
    Task<PublicKeyDto?> GetPublicKeyAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // INTERFACE: Rotate user's public key
    Task<Guid> RotateKeyAsync(Guid userId, string newPublicKey, CancellationToken cancellationToken = default);
    
    // INTERFACE: Revoke key (emergency)
    Task RevokeKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken = default);
    
    // INTERFACE: Get key history for user
    Task<List<KeyHistoryDto>> GetKeyHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
}

// ========================================
// DTOs
// ========================================

public record PublicKeyDto(
    Guid KeyId,
    Guid UserId,
    string PublicKey, // Base64-encoded
    DateTime CreatedAt,
    DateTime ExpiresAt,
    string Status // "active", "expired", "revoked"
);

public record KeyHistoryDto(
    Guid KeyId,
    string Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);
