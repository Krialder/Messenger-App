using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Data.Entities;

[Table("recovery_codes")]
public class RecoveryCode
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("code_hash")]
    [StringLength(255)]
    public string CodeHash { get; set; } = string.Empty;

    [Column("used")]
    public bool Used { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
