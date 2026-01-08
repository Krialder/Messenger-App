// ========================================
// PSEUDO-CODE - Sprint 6: Key Management Database Context
// Status: 🔶 Entity Framework Struktur
// ========================================

using Microsoft.EntityFrameworkCore;

namespace SecureMessenger.KeyManagementService.Data;

public class KeyDbContext : DbContext
{
    public KeyDbContext(DbContextOptions<KeyDbContext> options) : base(options)
    {
    }
    
    // PSEUDO: Define DbSets for entities
    public DbSet<PublicKey> PublicKeys => Set<PublicKey>();
    public DbSet<KeyAuditLog> KeyAuditLogs => Set<KeyAuditLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PSEUDO: Configure entities
        
        modelBuilder.Entity<PublicKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.IsActive });
            entity.HasIndex(e => e.ExpiresAt);
            
            entity.Property(e => e.PublicKeyBytes).IsRequired();
            entity.Property(e => e.KeyType).HasMaxLength(50);
        });
        
        modelBuilder.Entity<KeyAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
        });
    }
}

// ========================================
// ENTITIES
// ========================================

/// <summary>
/// Public key entity (maps to public_keys table from docs/05_DATA_MODEL.md)
/// </summary>
public class PublicKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public byte[] PublicKeyBytes { get; set; } = Array.Empty<byte>();
    public string KeyType { get; set; } = "x25519_e2e"; // x25519_e2e, x25519_signing, etc.
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
}

/// <summary>
/// Audit log for key operations (DSGVO compliance)
/// </summary>
public class KeyAuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid KeyId { get; set; }
    public string Action { get; set; } = string.Empty; // "created", "rotated", "revoked", "expired"
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
