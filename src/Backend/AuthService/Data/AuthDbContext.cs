using Microsoft.EntityFrameworkCore;

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
            // PSEUDO CODE: EF Core Configuration

            // User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.MasterKeySalt).IsRequired();
            });

            // MFA Methods entity
            modelBuilder.Entity<MfaMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Recovery Codes entity
            modelBuilder.Entity<RecoveryCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Refresh Tokens entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    // Entity Models
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public byte[] MasterKeySalt { get; set; } = Array.Empty<byte>();
        public bool MfaEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string AccountStatus { get; set; } = "active";
        public bool EmailVerified { get; set; }
    }

    public class MfaMethod
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string MethodType { get; set; } = string.Empty; // totp, yubikey, fido2
        public string? TotpSecret { get; set; }
        public string? YubikeyPublicId { get; set; }
        public byte[]? YubikeyCredentialId { get; set; }
        public byte[]? Fido2CredentialId { get; set; }
        public byte[]? Fido2PublicKey { get; set; }
        public bool IsPrimary { get; set; }
        public string? FriendlyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class RecoveryCode
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CodeHash { get; set; } = string.Empty;
        public bool Used { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
