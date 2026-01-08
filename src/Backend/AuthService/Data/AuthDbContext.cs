using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<MfaMethod> MfaMethods { get; set; }
        public DbSet<RecoveryCode> RecoveryCodes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users", t =>
                {
                    t.HasCheckConstraint("CK_User_Username_Length", "LENGTH(username) >= 3");
                    t.HasCheckConstraint("CK_User_AccountStatus", 
                        "account_status IN ('active', 'suspended', 'deleted')");
                });

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => new { e.IsActive, e.AccountStatus });

                entity.Property(e => e.MasterKeySalt)
                    .HasMaxLength(32)
                    .IsRequired();

                // Navigation properties
                entity.HasMany(u => u.MfaMethods)
                    .WithOne(m => m.User)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.RecoveryCodes)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.RefreshTokens)
                    .WithOne(t => t.User)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MfaMethod configuration
            modelBuilder.Entity<MfaMethod>(entity =>
            {
                entity.ToTable("mfa_methods", t =>
                {
                    t.HasCheckConstraint("CK_MfaMethod_Type",
                        "method_type IN ('totp', 'yubikey', 'fido2')");
                });

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.IsActive });
            });

            // RecoveryCode configuration
            modelBuilder.Entity<RecoveryCode>(entity =>
            {
                entity.ToTable("recovery_codes");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.Used });
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.IsRevoked });
                entity.HasIndex(e => e.Token)
                    .IsUnique()
                    .HasFilter("is_revoked = false");
            });
        }
    }

    // Entity Models
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("master_key_salt")]
        public byte[] MasterKeySalt { get; set; } = Array.Empty<byte>();

        [Column("mfa_enabled")]
        public bool MfaEnabled { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [MaxLength(20)]
        [Column("account_status")]
        public string AccountStatus { get; set; } = "active";

        [Column("email_verified")]
        public bool EmailVerified { get; set; } = false;

        [Column("deletion_requested_at")]
        public DateTime? DeletionRequestedAt { get; set; }

        [Column("deletion_scheduled_at")]
        public DateTime? DeletionScheduledAt { get; set; }

        // Navigation properties
        public virtual ICollection<MfaMethod> MfaMethods { get; set; } = new List<MfaMethod>();
        public virtual ICollection<RecoveryCode> RecoveryCodes { get; set; } = new List<RecoveryCode>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }

    [Table("mfa_methods")]
    public class MfaMethod
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("method_type")]
        public string MethodType { get; set; } = string.Empty;

        [Column("totp_secret")]
        public string? TotpSecret { get; set; }

        [Column("yubikey_public_id")]
        public string? YubikeyPublicId { get; set; }

        [Column("yubikey_credential_id")]
        public byte[]? YubikeyCredentialId { get; set; }

        [Column("fido2_credential_id")]
        public byte[]? Fido2CredentialId { get; set; }

        [Column("fido2_public_key")]
        public byte[]? Fido2PublicKey { get; set; }

        [Column("is_primary")]
        public bool IsPrimary { get; set; } = false;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        [Column("friendly_name")]
        public string? FriendlyName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_used_at")]
        public DateTime? LastUsedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

    [Table("recovery_codes")]
    public class RecoveryCode
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("code_hash")]
        public string CodeHash { get; set; } = string.Empty;

        [Column("used")]
        public bool Used { get; set; } = false;

        [Column("used_at")]
        public DateTime? UsedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("token")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
