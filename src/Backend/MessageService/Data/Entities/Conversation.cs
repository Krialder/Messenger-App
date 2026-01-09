using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageService.Data.Entities;

/// <summary>
/// Represents a conversation (1-to-1 chat or group chat)
/// </summary>
[Table("conversations")]
public class Conversation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("type")]
    public EntityConversationType Type { get; set; } = EntityConversationType.DirectMessage;

    /// <summary>
    /// Group name (null for DirectMessage conversations)
    /// </summary>
    [MaxLength(100)]
    [Column("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional description for group conversations
    /// </summary>
    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional group avatar/icon URL
    /// </summary>
    [MaxLength(255)]
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("encrypted_group_key")]
    public byte[]? EncryptedGroupKey { get; set; }

    // Navigation Properties
    public virtual ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

/// <summary>
/// Type of conversation
/// </summary>
public enum EntityConversationType
{
    /// <summary>
    /// 1-to-1 direct message
    /// </summary>
    DirectMessage = 0,

    /// <summary>
    /// Group chat (2+ members)
    /// </summary>
    Group = 1
}
