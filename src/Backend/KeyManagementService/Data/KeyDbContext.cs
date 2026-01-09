// ========================================
// Key Management Database Context
// Status: 🔶 Entity Framework Struktur
// ========================================

using KeyManagementService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyManagementService.Data;

/// <summary>
/// Database context for key management.
/// </summary>
public class KeyDbContext : DbContext
{
    public KeyDbContext(DbContextOptions<KeyDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// User public keys for E2E encryption.
    /// </summary>
    public DbSet<PublicKey> PublicKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PublicKey>(entity =>
        {
            entity.ToTable("public_keys");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.Key)
                .HasColumnName("public_key")
                .IsRequired()
                .HasMaxLength(32);

            entity.Property(e => e.Algorithm)
                .HasColumnName("algorithm")
                .HasMaxLength(50)
                .HasDefaultValue("X25519");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasDefaultValueSql("NOW() + INTERVAL '1 year'");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at");

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_public_keys_user_id");

            entity.HasIndex(e => new { e.UserId, e.IsActive })
                .HasDatabaseName("idx_public_keys_user_active");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("idx_public_keys_expires_at");
        });
    }
}
