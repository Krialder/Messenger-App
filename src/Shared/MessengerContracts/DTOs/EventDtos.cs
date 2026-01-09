namespace MessengerContracts.DTOs;

/// <summary>
/// Event fired when a message is sent.
/// </summary>
public class MessageSentEvent
{
    /// <summary>
    /// Message ID.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Sender user ID.
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Conversation ID (for both direct and group messages).
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Recipient user IDs (for direct messages or group members).
    /// </summary>
    public List<Guid> RecipientIds { get; set; } = new();

    /// <summary>
    /// Timestamp when message was sent.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Message type (0=Text, 1=Image, 2=File, etc.).
    /// </summary>
    public int MessageType { get; set; }
}

/// <summary>
/// Event fired when a message is delivered.
/// </summary>
public class MessageDeliveredEvent
{
    /// <summary>
    /// Message ID.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// User ID who received the message.
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    /// Delivery timestamp.
    /// </summary>
    public DateTime DeliveredAt { get; set; }
}

/// <summary>
/// Event fired when a message is read.
/// </summary>
public class MessageReadEvent
{
    /// <summary>
    /// Message ID.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// User ID who read the message.
    /// </summary>
    public Guid ReadBy { get; set; }

    /// <summary>
    /// Read timestamp.
    /// </summary>
    public DateTime ReadAt { get; set; }
}

/// <summary>
/// Event fired when a user starts typing.
/// </summary>
public class UserTypingEvent
{
    /// <summary>
    /// User ID who is typing.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Conversation ID.
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Whether the user is currently typing.
    /// </summary>
    public bool IsTyping { get; set; }

    /// <summary>
    /// Timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Event fired when a user's online status changes.
/// </summary>
public class UserOnlineEvent
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Whether the user is online.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Event fired when a cryptographic key is rotated.
/// </summary>
public class KeyRotationEvent
{
    /// <summary>
    /// User ID whose key was rotated.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// New key ID.
    /// </summary>
    public Guid NewKeyId { get; set; }

    /// <summary>
    /// Old key ID (if applicable).
    /// </summary>
    public Guid? OldKeyId { get; set; }

    /// <summary>
    /// Rotation timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
