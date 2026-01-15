using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Data.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("master_key_salt")]
    public byte[] MasterKeySalt { get; set; } = new byte[32];

    [Column("email_verified")]
    public bool EmailVerified { get; set; } = false;

    [Column("mfa_enabled")]
    public bool MfaEnabled { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("account_status")]
    [StringLength(20)]
    public string AccountStatus { get; set; } = "active"; // active, suspended, locked, deleted

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [Column("delete_scheduled_at")]
    public DateTime? DeleteScheduledAt { get; set; }

    // Navigation properties
    public virtual ICollection<MfaMethod> MfaMethods { get; set; } = new List<MfaMethod>();
    public virtual ICollection<RecoveryCode> RecoveryCodes { get; set; } = new List<RecoveryCode>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
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
