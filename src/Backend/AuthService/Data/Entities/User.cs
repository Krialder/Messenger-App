using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Data.Entities;

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
    public string MethodType { get; set; } = string.Empty; // "totp", "yubikey", "fido2"

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
