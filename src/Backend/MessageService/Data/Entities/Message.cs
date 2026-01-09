using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageService.Data.Entities;

/// <summary>
/// Represents an encrypted message in a conversation
/// </summary>
[Table("messages")]
public class Message
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Conversation this message belongs to
    /// </summary>
    [Required]
    [Column("conversation_id")]
    public Guid ConversationId { get; set; }

    /// <summary>
    /// User who sent the message
    /// </summary>
    [Required]
    [Column("sender_id")]
    public Guid SenderId { get; set; }

    /// <summary>
    /// Encrypted message content (Layer 1 E2E encryption)
    /// </summary>
    [Required]
    [Column("encrypted_content")]
    public byte[] EncryptedContent { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Nonce for encryption (12 bytes for ChaCha20)
    /// </summary>
    [Required]
    [Column("nonce")]
    public byte[] Nonce { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Ephemeral public key for Layer 1 encryption
    /// </summary>
    [Column("ephemeral_public_key")]
    public byte[]? EphemeralPublicKey { get; set; }

    /// <summary>
    /// For group messages: encrypted group key for each member
    /// JSON format: [{"userId": "...", "encryptedGroupKey": "..."}]
    /// </summary>
    [Column("encrypted_group_keys", TypeName = "jsonb")]
    public string? EncryptedGroupKeys { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Message status
    /// </summary>
    [Column("status")]
    public EntityMessageStatus Status { get; set; } = EntityMessageStatus.Sent;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Message type (text, image, file, etc.)
    /// </summary>
    [Column("type")]
    public EntityMessageType Type { get; set; } = EntityMessageType.Text;

    /// <summary>
    /// Optional: Reply to another message
    /// </summary>
    [Column("reply_to_message_id")]
    public Guid? ReplyToMessageId { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey(nameof(ReplyToMessageId))]
    public virtual Message? ReplyToMessage { get; set; }
}

/// <summary>
/// Message delivery/read status
/// </summary>
public enum EntityMessageStatus
{
    Sent = 0,
    Delivered = 1,
    Read = 2
}

/// <summary>
/// Type of message content
/// </summary>
public enum EntityMessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    Voice = 3,
    Video = 4,
    SystemNotification = 5  // e.g., "Alice added Bob to the group"
}
