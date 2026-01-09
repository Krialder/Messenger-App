using System;
using System.Collections.Generic;

namespace MessengerContracts.DTOs
{
    /// <summary>
    /// Data Transfer Object for messages (supports both 1-to-1 and group messages)
    /// </summary>
    public class MessageDto
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Conversation this message belongs to
        /// </summary>
        public Guid ConversationId { get; set; }
        
        public Guid SenderId { get; set; }
        
        /// <summary>
        /// DEPRECATED: Use ConversationId instead. Kept for backward compatibility with 1-to-1 chats.
        /// </summary>
        [Obsolete("Use ConversationId for both direct and group messages")]
        public Guid? RecipientId { get; set; }
        
        /// <summary>
        /// Base64-encoded encrypted content
        /// </summary>
        public string EncryptedContent { get; set; } = string.Empty;
        
        /// <summary>
        /// Base64-encoded nonce
        /// </summary>
        public string? Nonce { get; set; }
        
        /// <summary>
        /// Base64-encoded ephemeral public key (for Layer 1 encryption)
        /// </summary>
        public string? EphemeralPublicKey { get; set; }
        
        /// <summary>
        /// For group messages: encrypted group key for each member
        /// </summary>
        public List<EncryptedGroupKeyDto>? EncryptedGroupKeys { get; set; }
        
        public DateTime Timestamp { get; set; }
        public MessageStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        
        /// <summary>
        /// ID of message being replied to (optional)
        /// </summary>
        public Guid? ReplyToMessageId { get; set; }
    }

    /// <summary>
    /// Encrypted group key for a specific user
    /// </summary>
    public record EncryptedGroupKeyDto(
        Guid UserId,
        string EncryptedGroupKey  // Base64-encoded
    );

    /// <summary>
    /// Request DTO for sending a message
    /// </summary>
    public record SendMessageRequest(
        Guid ConversationId,
        byte[] EncryptedContent,
        byte[] Nonce,
        byte[]? EphemeralPublicKey,
        List<EncryptedGroupKeyDto>? EncryptedGroupKeys = null,
        MessageType Type = MessageType.Text,
        Guid? ReplyToMessageId = null
    );

    public enum MessageStatus
    {
        Sent,
        Delivered,
        Read
    }

    public enum MessageType
    {
        Text = 0,
        Image = 1,
        File = 2,
        Voice = 3,
        Video = 4,
        SystemNotification = 5
    }
}
