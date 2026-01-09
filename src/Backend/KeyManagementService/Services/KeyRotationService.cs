// ========================================
// PSEUDO-CODE - Sprint 6: Key Rotation Service
// Status: 🔶 Interface definiert
// Background Service: Automatische Schlüsselrotation
// ========================================

using KeyManagementService.Data;
using KeyManagementService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyManagementService.Services;

/// <summary>
/// Service for managing cryptographic key rotation.
/// </summary>
public interface IKeyRotationService
{
    /// <summary>
    /// Rotates expired keys (older than 1 year).
    /// </summary>
    Task RotateExpiredKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a specific user's key manually.
    /// </summary>
    Task<PublicKey> RotateUserKeyAsync(Guid userId, byte[] newPublicKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific key.
    /// </summary>
    Task RevokeKeyAsync(Guid keyId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of key rotation service.
/// </summary>
public class KeyRotationService : IKeyRotationService
{
    private readonly KeyDbContext _context;
    private readonly ILogger<KeyRotationService> _logger;

    public KeyRotationService(KeyDbContext context, ILogger<KeyRotationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Rotates all keys that have expired (older than 1 year).
    /// </summary>
    public async Task RotateExpiredKeysAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        // Find all expired keys that are still active
        List<PublicKey> expiredKeys = await _context.PublicKeys
            .Where(k => k.IsActive && k.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (expiredKeys.Count == 0)
        {
            _logger.LogInformation("No expired keys found for rotation.");
            return;
        }

        _logger.LogInformation("Found {Count} expired keys for rotation.", expiredKeys.Count);

        // Mark expired keys as inactive
        foreach (PublicKey key in expiredKeys)
        {
            key.IsActive = false;
            key.RevokedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully rotated {Count} expired keys.", expiredKeys.Count);
    }

    /// <summary>
    /// Manually rotates a user's key.
    /// </summary>
    public async Task<PublicKey> RotateUserKeyAsync(Guid userId, byte[] newPublicKey, CancellationToken cancellationToken = default)
    {
        if (newPublicKey == null || newPublicKey.Length != 32)
        {
            throw new ArgumentException("Public key must be 32 bytes.", nameof(newPublicKey));
        }

        // Deactivate old keys
        List<PublicKey> oldKeys = await _context.PublicKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .ToListAsync(cancellationToken);

        DateTime now = DateTime.UtcNow;

        foreach (PublicKey oldKey in oldKeys)
        {
            oldKey.IsActive = false;
            oldKey.RevokedAt = now;
        }

        // Create new key
        PublicKey newKey = new PublicKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Key = newPublicKey,
            Algorithm = "X25519",
            CreatedAt = now,
            ExpiresAt = now.AddYears(1),
            IsActive = true
        };

        _context.PublicKeys.Add(newKey);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Rotated key for user {UserId}. Old keys: {OldCount}, New key: {NewKeyId}",
            userId, oldKeys.Count, newKey.Id);

        return newKey;
    }

    /// <summary>
    /// Revokes a specific key.
    /// </summary>
    public async Task RevokeKeyAsync(Guid keyId, CancellationToken cancellationToken = default)
    {
        PublicKey? key = await _context.PublicKeys
            .FirstOrDefaultAsync(k => k.Id == keyId, cancellationToken);

        if (key == null)
        {
            throw new KeyNotFoundException($"Key with ID {keyId} not found.");
        }

        if (!key.IsActive)
        {
            _logger.LogWarning("Key {KeyId} is already revoked.", keyId);
            return;
        }

        key.IsActive = false;
        key.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked key {KeyId} for user {UserId}.", keyId, key.UserId);
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
