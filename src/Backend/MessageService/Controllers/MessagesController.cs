using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerContracts.DTOs;
using MessageService.Data;
using MessageService.Data.Entities;
using MessageService.Services;
using DtoConversationType = MessengerContracts.DTOs.ConversationType;
using DtoMessageStatus = MessengerContracts.DTOs.MessageStatus;
using DtoMessageType = MessengerContracts.DTOs.MessageType;

namespace MessageService.Controllers
{
    /// <summary>
    /// API Controller for messaging (supports both direct and group messages)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly MessageDbContext _context;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            MessageDbContext context,
            IRabbitMQService rabbitMQService,
            ILogger<MessagesController> logger)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        /// <summary>
        /// Send a message to a conversation (direct or group)
        /// POST /api/messages
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderId = GetCurrentUserId();

            try
            {
                // 1. Validate conversation exists and user is a member
                var conversation = await _context.Conversations
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

                if (conversation == null)
                    return NotFound("Conversation not found");

                // Check if sender is a member
                var senderMembership = conversation.Members
                    .FirstOrDefault(m => m.UserId == senderId && m.LeftAt == null);
                
                if (senderMembership == null)
                    return Forbid("You are not a member of this conversation");

                // 2. Create message entity
                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = request.ConversationId,
                    SenderId = senderId,
                    EncryptedContent = request.EncryptedContent,
                    Nonce = request.Nonce,
                    EphemeralPublicKey = request.EphemeralPublicKey,
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityMessageStatus.Sent,
                    Type = (EntityMessageType)request.Type
                };

                // 3. For group messages: Store encrypted group keys
                if (conversation.Type == EntityConversationType.Group && request.EncryptedGroupKeys != null)
                {
                    // Store as JSONB in PostgreSQL
                    message.EncryptedGroupKeys = System.Text.Json.JsonSerializer.Serialize(
                        request.EncryptedGroupKeys);
                }

                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Message sent: {MessageId} in conversation {ConversationId} by {SenderId}",
                    message.Id, request.ConversationId, senderId);

                // 4. Publish to RabbitMQ for real-time delivery
                var recipientIds = conversation.Members
                    .Where(m => m.UserId != senderId && m.LeftAt == null)
                    .Select(m => m.UserId)
                    .ToList();

                await _rabbitMQService.PublishMessageSentEventAsync(new MessageSentEvent
                {
                    MessageId = message.Id,
                    SenderId = senderId,
                    ConversationId = request.ConversationId,
                    RecipientIds = recipientIds,
                    Timestamp = message.CreatedAt,
                    MessageType = (int)request.Type
                });

                _logger.LogDebug("Published MessageSentEvent to RabbitMQ for message {MessageId}", message.Id);

                return Ok(new
                {
                    messageId = message.Id,
                    timestamp = message.CreatedAt,
                    status = message.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, "Error sending message");
            }
        }

        /// <summary>
        /// Get message history for a conversation
        /// GET /api/messages/conversation/{conversationId}
        /// </summary>
        [HttpGet("conversation/{conversationId}")]
        public async Task<IActionResult> GetConversationMessages(
            Guid conversationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var userId = GetCurrentUserId();

            if (pageSize > 100) pageSize = 100; // Max limit

            try
            {
                // 1. Verify user is a member of the conversation
                var isMember = await _context.ConversationMembers
                    .AnyAsync(m => m.ConversationId == conversationId && m.UserId == userId && m.LeftAt == null);

                if (!isMember)
                    return Forbid("You are not a member of this conversation");

                // 2. Fetch messages (paginated)
                var query = _context.Messages
                    .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt);

                var totalCount = await query.CountAsync();
                var messages = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        ConversationId = m.ConversationId,
                        SenderId = m.SenderId,
                        EncryptedContent = Convert.ToBase64String(m.EncryptedContent),
                        Nonce = m.Nonce != null ? Convert.ToBase64String(m.Nonce) : null,
                        EphemeralPublicKey = m.EphemeralPublicKey != null ? Convert.ToBase64String(m.EphemeralPublicKey) : null,
                        Timestamp = m.CreatedAt,
                        Status = (DtoMessageStatus)m.Status,
                        IsDeleted = m.IsDeleted,
                        Type = (DtoMessageType)m.Type,
                        ReplyToMessageId = m.ReplyToMessageId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    messages,
                    totalCount,
                    page,
                    pageSize,
                    hasMore = (page * pageSize) < totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for conversation {ConversationId}", conversationId);
                return StatusCode(500, "Error fetching messages");
            }
        }

        /// <summary>
        /// Get unread message count for all conversations
        /// GET /api/messages/unread
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadMessages()
        {
            var userId = GetCurrentUserId();

            try
            {
                // Get all conversations where user is a member
                var memberConversations = await _context.ConversationMembers
                    .Where(m => m.UserId == userId && m.LeftAt == null)
                    .Select(m => new
                    {
                        m.ConversationId,
                        m.LastReadAt
                    })
                    .ToListAsync();

                var unreadCounts = new List<object>();

                foreach (var membership in memberConversations)
                {
                    var unreadCount = await _context.Messages
                        .Where(m => m.ConversationId == membership.ConversationId
                                 && m.SenderId != userId
                                 && !m.IsDeleted
                                 && (membership.LastReadAt == null || m.CreatedAt > membership.LastReadAt))
                        .CountAsync();

                    if (unreadCount > 0)
                    {
                        unreadCounts.Add(new
                        {
                            conversationId = membership.ConversationId,
                            unreadCount
                        });
                    }
                }

                return Ok(new
                {
                    unreadConversations = unreadCounts.Count,
                    conversations = unreadCounts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread messages");
                return StatusCode(500, "Error fetching unread messages");
            }
        }

        /// <summary>
        /// Mark messages as read in a conversation
        /// PUT /api/messages/conversation/{conversationId}/read
        /// </summary>
        [HttpPut("conversation/{conversationId}/read")]
        public async Task<IActionResult> MarkConversationAsRead(Guid conversationId)
        {
            var userId = GetCurrentUserId();

            try
            {
                var membership = await _context.ConversationMembers
                    .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId && m.LeftAt == null);

                if (membership == null)
                    return NotFound("Membership not found");

                membership.LastReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Publish MessageReadEvent to RabbitMQ
                await _rabbitMQService.PublishMessageReadEventAsync(new MessageReadEvent
                {
                    MessageId = conversationId, // Using conversation ID for simplicity
                    ReadBy = userId,
                    ReadAt = membership.LastReadAt.Value
                });

                _logger.LogDebug("Published MessageReadEvent to RabbitMQ for conversation {ConversationId}", conversationId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation as read");
                return StatusCode(500, "Error updating read status");
            }
        }

        /// <summary>
        /// Delete a message (soft delete)
        /// DELETE /api/messages/{messageId}
        /// </summary>
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = GetCurrentUserId();

            try
            {
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null)
                    return NotFound("Message not found");

                // Only sender can delete their own messages
                if (message.SenderId != userId)
                    return Forbid("You can only delete your own messages");

                // Soft delete
                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Message {MessageId} deleted by {UserId}", messageId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                return StatusCode(500, "Error deleting message");
            }
        }

        // ========================================
        // Helper Methods
        // ========================================

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
            return Guid.Parse(userIdClaim!);
        }
    }
}
