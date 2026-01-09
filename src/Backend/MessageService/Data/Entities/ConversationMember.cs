using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageService.Data.Entities;

/// <summary>
/// Represents a user's membership in a conversation
/// </summary>
[Table("conversation_members")]
public class ConversationMember
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("conversation_id")]
    public Guid ConversationId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("role")]
    public EntityMemberRole Role { get; set; } = EntityMemberRole.Member;

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user left the conversation (null if still active)
    /// </summary>
    [Column("left_at")]
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// Last read message timestamp (for unread count)
    /// </summary>
    [Column("last_read_at")]
    public DateTime? LastReadAt { get; set; }

    /// <summary>
    /// Is the member muted in this conversation
    /// </summary>
    [Column("is_muted")]
    public bool IsMuted { get; set; } = false;

    /// <summary>
    /// Custom nickname in this conversation
    /// </summary>
    [MaxLength(100)]
    [Column("custom_nickname")]
    public string? CustomNickname { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; } = null!;
}

/// <summary>
/// Role of a member in a group conversation
/// </summary>
public enum EntityMemberRole
{
    /// <summary>
    /// Owner (creator of the group, cannot be removed)
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Admin (can add/remove members, change group settings)
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Regular member (can send/read messages)
    /// </summary>
    Member = 2
}
