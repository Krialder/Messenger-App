using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificationService.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications.
/// Handles: Message notifications, typing indicators, presence management, group events.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    // CONNECTION LIFECYCLE

    public override async Task OnConnectedAsync()
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} connected to NotificationHub", userId);

            // Notify others that user is online
            await Clients.Others.SendAsync("UserOnline", new { userId, timestamp = DateTime.UtcNow });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);

            // Notify others that user is offline
            await Clients.Others.SendAsync("UserOffline", new { userId, timestamp = DateTime.UtcNow });
        }

        await base.OnDisconnectedAsync(exception);
    }

    // GROUP MANAGEMENT

    /// <summary>
    /// Join a conversation group (for group chats).
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

        _logger.LogDebug("User {UserId} joined conversation {ConversationId}", userId, conversationId);

        // Notify group members
        await Clients.Group($"conversation_{conversationId}")
            .SendAsync("UserJoinedConversation", new { userId, conversationId });
    }

    /// <summary>
    /// Leave a conversation group.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

        _logger.LogDebug("User {UserId} left conversation {ConversationId}", userId, conversationId);

        // Notify group members
        await Clients.Group($"conversation_{conversationId}")
            .SendAsync("UserLeftConversation", new { userId, conversationId });
    }

    // TYPING INDICATORS

    /// <summary>
    /// Notify conversation members that user is typing.
    /// </summary>
    public async Task NotifyTyping(string conversationId, bool isTyping)
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId)) return;

        // Notify all group members except sender
        await Clients.OthersInGroup($"conversation_{conversationId}")
            .SendAsync("UserTyping", new { userId, conversationId, isTyping, timestamp = DateTime.UtcNow });

        _logger.LogDebug("User {UserId} typing status: {IsTyping} in conversation {ConversationId}",
            userId, isTyping, conversationId);
    }

    // MESSAGE DELIVERY CONFIRMATION

    /// <summary>
    /// Confirm message delivery.
    /// </summary>
    public async Task ConfirmMessageDelivery(string messageId)
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogDebug("User {UserId} confirmed delivery of message {MessageId}", userId, messageId);

        // This could trigger a MessageDeliveredEvent to RabbitMQ
        // For now, just log
    }

    /// <summary>
    /// Confirm message read.
    /// </summary>
    public async Task ConfirmMessageRead(string messageId)
    {
        string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogDebug("User {UserId} confirmed reading message {MessageId}", userId, messageId);

        // This could trigger a MessageReadEvent to RabbitMQ
        // For now, just log
    }
}
