using KeyManagementService.Data;
using KeyManagementService.Data.Entities;
using KeyManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MessengerTests.ServiceTests;

/// <summary>
/// Unit tests for KeyManagementService - Key rotation and lifecycle management.
/// </summary>
public class KeyManagementServiceTests : IDisposable
{
    private readonly KeyDbContext _context;
    private readonly IKeyRotationService _keyRotationService;
    private readonly ILogger<KeyRotationService> _logger;

    public KeyManagementServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<KeyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new KeyDbContext(options);

        // Setup logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<KeyRotationService>();

        // Setup service
        _keyRotationService = new KeyRotationService(_context, _logger);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var activeKey = new PublicKey
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Key = new byte[32],
            Algorithm = "X25519",
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            ExpiresAt = DateTime.UtcNow.AddMonths(6),
            IsActive = true
        };

        var expiredKey = new PublicKey
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Key = new byte[32],
            Algorithm = "X25519",
            CreatedAt = DateTime.UtcNow.AddYears(-2),
            ExpiresAt = DateTime.UtcNow.AddYears(-1),
            IsActive = true
        };

        var revokedKey = new PublicKey
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            UserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Key = new byte[32],
            Algorithm = "X25519",
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            ExpiresAt = DateTime.UtcNow.AddMonths(9),
            IsActive = false,
            RevokedAt = DateTime.UtcNow.AddMonths(-1)
        };

        _context.PublicKeys.AddRange(activeKey, expiredKey, revokedKey);
        _context.SaveChanges();
    }

    // ========================================
    // KEY ROTATION TESTS
    // ========================================

    [Fact]
    public async Task RotateExpiredKeysAsync_ExpiredKeyExists_DeactivatesKey()
    {
        // Act
        await _keyRotationService.RotateExpiredKeysAsync();

        // Assert
        var expiredKey = await _context.PublicKeys
            .FirstOrDefaultAsync(k => k.Id == Guid.Parse("22222222-2222-2222-2222-222222222222"));

        Assert.NotNull(expiredKey);
        Assert.False(expiredKey.IsActive);
        Assert.NotNull(expiredKey.RevokedAt);
        Assert.True(expiredKey.RevokedAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task RotateExpiredKeysAsync_NoExpiredKeys_NoChanges()
    {
        // Arrange - Mark expired key as inactive
        var expiredKey = await _context.PublicKeys
            .FirstOrDefaultAsync(k => k.Id == Guid.Parse("22222222-2222-2222-2222-222222222222"));
        
        if (expiredKey != null)
        {
            expiredKey.IsActive = false;
            await _context.SaveChangesAsync();
        }

        var activeCountBefore = await _context.PublicKeys.CountAsync(k => k.IsActive);

        // Act
        await _keyRotationService.RotateExpiredKeysAsync();

        // Assert
        var activeCountAfter = await _context.PublicKeys.CountAsync(k => k.IsActive);
        Assert.Equal(activeCountBefore, activeCountAfter);
    }

    [Fact]
    public async Task RotateExpiredKeysAsync_MultipleExpiredKeys_DeactivatesAll()
    {
        // Arrange - Add another expired key
        var expiredKey2 = new PublicKey
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Key = new byte[32],
            Algorithm = "X25519",
            CreatedAt = DateTime.UtcNow.AddYears(-3),
            ExpiresAt = DateTime.UtcNow.AddYears(-2),
            IsActive = true
        };

        _context.PublicKeys.Add(expiredKey2);
        await _context.SaveChangesAsync();

        // Act
        await _keyRotationService.RotateExpiredKeysAsync();

        // Assert
        var expiredKeys = await _context.PublicKeys
            .Where(k => k.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        Assert.All(expiredKeys, key => Assert.False(key.IsActive));
    }

    // ========================================
    // MANUAL KEY ROTATION TESTS
    // ========================================

    [Fact]
    public async Task RotateUserKeyAsync_ValidKey_CreatesNewKey()
    {
        // Arrange
        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var newPublicKey = new byte[32];
        new Random().NextBytes(newPublicKey);

        // Act
        var result = await _keyRotationService.RotateUserKeyAsync(userId, newPublicKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(newPublicKey, result.Key);
        Assert.True(result.IsActive);
        Assert.Equal("X25519", result.Algorithm);
        Assert.True(result.CreatedAt >= DateTime.UtcNow.AddMinutes(-1));
        Assert.True(result.ExpiresAt >= DateTime.UtcNow.AddYears(1).AddMinutes(-1));
    }

    [Fact]
    public async Task RotateUserKeyAsync_ValidKey_DeactivatesOldKeys()
    {
        // Arrange
        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var newPublicKey = new byte[32];
        new Random().NextBytes(newPublicKey);

        // Get old key count
        var oldActiveKeys = await _context.PublicKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .ToListAsync();

        Assert.Single(oldActiveKeys);

        // Act
        var result = await _keyRotationService.RotateUserKeyAsync(userId, newPublicKey);

        // Assert - Old keys should be deactivated
        var oldKey = await _context.PublicKeys
            .FirstOrDefaultAsync(k => k.Id == Guid.Parse("11111111-1111-1111-1111-111111111111"));

        Assert.NotNull(oldKey);
        Assert.False(oldKey.IsActive);
        Assert.NotNull(oldKey.RevokedAt);

        // New key should be active
        var newKey = await _context.PublicKeys
            .FirstOrDefaultAsync(k => k.Id == result.Id);

        Assert.NotNull(newKey);
        Assert.True(newKey.IsActive);
    }

    [Fact]
    public async Task RotateUserKeyAsync_InvalidKeySize_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invalidKey = new byte[16]; // Wrong size

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _keyRotationService.RotateUserKeyAsync(userId, invalidKey));
    }

    [Fact]
    public async Task RotateUserKeyAsync_NullKey_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _keyRotationService.RotateUserKeyAsync(userId, null!));
    }

    [Fact]
    public async Task RotateUserKeyAsync_NewUser_CreatesFirstKey()
    {
        // Arrange
        var newUserId = Guid.NewGuid();
        var newPublicKey = new byte[32];
        new Random().NextBytes(newPublicKey);

        // Act
        var result = await _keyRotationService.RotateUserKeyAsync(newUserId, newPublicKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newUserId, result.UserId);
        Assert.True(result.IsActive);

        // Verify in database
        var keysInDb = await _context.PublicKeys
            .Where(k => k.UserId == newUserId)
            .ToListAsync();

        Assert.Single(keysInDb);
    }

    // ========================================
    // KEY REVOCATION TESTS
    // ========================================

    [Fact]
    public async Task RevokeKeyAsync_ActiveKey_RevokesSuccessfully()
    {
        // Arrange
        var keyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        await _keyRotationService.RevokeKeyAsync(keyId);

        // Assert
        var key = await _context.PublicKeys.FindAsync(keyId);
        Assert.NotNull(key);
        Assert.False(key.IsActive);
        Assert.NotNull(key.RevokedAt);
        Assert.True(key.RevokedAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task RevokeKeyAsync_AlreadyRevokedKey_NoError()
    {
        // Arrange
        var keyId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Verify key is already revoked
        var keyBefore = await _context.PublicKeys.FindAsync(keyId);
        Assert.NotNull(keyBefore);
        Assert.False(keyBefore.IsActive);

        // Act - Should not throw
        await _keyRotationService.RevokeKeyAsync(keyId);

        // Assert - Should still be revoked
        var keyAfter = await _context.PublicKeys.FindAsync(keyId);
        Assert.NotNull(keyAfter);
        Assert.False(keyAfter.IsActive);
    }

    [Fact]
    public async Task RevokeKeyAsync_NonExistentKey_ThrowsException()
    {
        // Arrange
        var nonExistentKeyId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _keyRotationService.RevokeKeyAsync(nonExistentKeyId));
    }

    // ========================================
    // KEY LIFECYCLE TESTS
    // ========================================

    [Fact]
    public async Task KeyLifecycle_CreateRotateRevoke_Works()
    {
        // Step 1: Create initial key
        var userId = Guid.NewGuid();
        var key1 = new byte[32];
        new Random().NextBytes(key1);

        var initialKey = await _keyRotationService.RotateUserKeyAsync(userId, key1);
        Assert.True(initialKey.IsActive);

        // Step 2: Rotate to new key
        var key2 = new byte[32];
        new Random().NextBytes(key2);

        var rotatedKey = await _keyRotationService.RotateUserKeyAsync(userId, key2);
        Assert.True(rotatedKey.IsActive);

        // Verify old key is deactivated
        var oldKey = await _context.PublicKeys.FindAsync(initialKey.Id);
        Assert.NotNull(oldKey);
        Assert.False(oldKey.IsActive);

        // Step 3: Revoke current key
        await _keyRotationService.RevokeKeyAsync(rotatedKey.Id);

        var revokedKey = await _context.PublicKeys.FindAsync(rotatedKey.Id);
        Assert.NotNull(revokedKey);
        Assert.False(revokedKey.IsActive);
        Assert.NotNull(revokedKey.RevokedAt);

        // Verify user has no active keys
        var activeKeys = await _context.PublicKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .ToListAsync();

        Assert.Empty(activeKeys);
    }

    [Fact]
    public async Task GetUserActiveKey_ActiveKeyExists_ReturnsKey()
    {
        // Arrange
        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var activeKey = await _context.PublicKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(activeKey);
        Assert.Equal("X25519", activeKey.Algorithm);
    }

    [Fact]
    public async Task GetUserActiveKey_NoActiveKey_ReturnsNull()
    {
        // Arrange
        var userId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        // Act
        var activeKey = await _context.PublicKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .FirstOrDefaultAsync();

        // Assert
        Assert.Null(activeKey);
    }

    // ========================================
    // EXPIRATION TESTS
    // ========================================

    [Fact]
    public async Task CheckKeyExpiration_KeyExpiresInOneYear()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newKey = new byte[32];
        new Random().NextBytes(newKey);

        // Act
        var key = await _keyRotationService.RotateUserKeyAsync(userId, newKey);

        // Assert
        var expectedExpiration = DateTime.UtcNow.AddYears(1);
        Assert.True(key.ExpiresAt >= expectedExpiration.AddMinutes(-1));
        Assert.True(key.ExpiresAt <= expectedExpiration.AddMinutes(1));
    }

    [Fact]
    public async Task GetExpiringKeys_WithinSevenDays_ReturnsKeys()
    {
        // Arrange - Create a key expiring in 5 days
        var soonToExpireKey = new PublicKey
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Key = new byte[32],
            Algorithm = "X25519",
            CreatedAt = DateTime.UtcNow.AddYears(-1).AddDays(2),
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };

        _context.PublicKeys.Add(soonToExpireKey);
        await _context.SaveChangesAsync();

        // Act
        var expiringKeys = await _context.PublicKeys
            .Where(k => k.IsActive && k.ExpiresAt <= DateTime.UtcNow.AddDays(7))
            .ToListAsync();

        // Assert
        Assert.Contains(expiringKeys, k => k.Id == soonToExpireKey.Id);
    }

    // ========================================
    // CONCURRENT ACCESS TESTS
    // ========================================

    [Fact]
    public async Task RotateUserKey_ConcurrentRotation_HandlesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var key1 = new byte[32];
        var key2 = new byte[32];
        new Random().NextBytes(key1);
        new Random().NextBytes(key2);

        // Act - Simulate concurrent rotations
        var task1 = _keyRotationService.RotateUserKeyAsync(userId, key1);
        var task2 = _keyRotationService.RotateUserKeyAsync(userId, key2);

        var results = await Task.WhenAll(task1, task2);

        // Assert - Both should succeed
        Assert.NotNull(results[0]);
        Assert.NotNull(results[1]);

        // Verify final state - only one should be active
        var activeKeys = await _context.PublicKeys
            .Where(k => k.UserId == userId && k.IsActive)
            .ToListAsync();

        Assert.Single(activeKeys);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
