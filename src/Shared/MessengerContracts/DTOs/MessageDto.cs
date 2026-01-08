using System;

namespace MessengerContracts.DTOs
{
    /// <summary>
    /// Data Transfer Object for messages
    /// </summary>
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public MessageStatus Status { get; set; }
        public bool IsDeleted { get; set; }
    }

    public enum MessageStatus
    {
        Sent,
        Delivered,
        Read
    }
}
