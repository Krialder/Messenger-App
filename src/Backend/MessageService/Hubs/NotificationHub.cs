using Microsoft.AspNetCore.SignalR;

namespace MessageService.Hubs
{
    /// <summary>
    /// SignalR Hub for Real-time Notifications
    /// Handles message delivery, typing indicators, online status
    /// Supports both direct messages and group chats
    /// </summary>
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
            var userId = Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Add connection to user's personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                
                // TODO: Update online status in Redis
                // await _presenceService.SetUserOnlineAsync(userId);
                
                // Broadcast online status to contacts
                await Clients.Others.SendAsync("UserOnline", userId);
                
                _logger.LogInformation("User {UserId} connected: {ConnectionId}", userId, Context.ConnectionId);
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // TODO: Update offline status in Redis
                // await _presenceService.SetUserOfflineAsync(userId);
                
                // Broadcast offline status
                await Clients.Others.SendAsync("UserOffline", userId);
                
                _logger.LogInformation("User {UserId} disconnected: {ConnectionId}", userId, Context.ConnectionId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // TYPING INDICATORS

        /// <summary>
        /// Send typing indicator to a specific user (1-to-1 chat)
        /// </summary>
        public async Task SendTypingIndicator(string recipientId)
        {
            var senderId = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(senderId)) return;

            await Clients.User(recipientId).SendAsync("TypingIndicator", senderId);
        }

        /// <summary>
        /// Send typing indicator to a group conversation
        /// </summary>
        public async Task SendGroupTypingIndicator(string conversationId)
        {
            var senderId = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(senderId)) return;

            // Broadcast to all members in the conversation group (except sender)
            await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
                .SendAsync("GroupTypingIndicator", conversationId, senderId);
        }

        public async Task StopTypingIndicator(string recipientId)
        {
            var senderId = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(senderId)) return;

            await Clients.User(recipientId).SendAsync("StopTyping", senderId);
        }

        // MESSAGING EVENTS

        /// <summary>
        /// Notify a single recipient of a new message (1-to-1)
        /// </summary>
        public async Task NotifyNewMessage(string recipientId, object messageData)
        {
            await Clients.User(recipientId).SendAsync("NewMessage", messageData);
            _logger.LogDebug("Notified user {RecipientId} of new message", recipientId);
        }

        /// <summary>
        /// Notify all members of a group conversation about a new message
        /// </summary>
        public async Task NotifyGroupMessage(List<string> memberIds, object messageData)
        {
            foreach (var memberId in memberIds)
            {
                await Clients.User(memberId).SendAsync("NewMessage", messageData);
            }
            
            _logger.LogDebug("Notified {MemberCount} group members of new message", memberIds.Count);
        }

        /// <summary>
        /// Notify message sender that their message was read
        /// </summary>
        public async Task NotifyMessageRead(string senderId, string messageId)
        {
            await Clients.User(senderId).SendAsync("MessageRead", messageId);
        }

        // GROUP MANAGEMENT EVENTS

        /// <summary>
        /// Join a conversation group (subscribe to real-time updates)
        /// </summary>
        public async Task JoinConversationGroup(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation("User {UserId} joined conversation group {ConversationId}", 
                userId, conversationId);
        }

        /// <summary>
        /// Leave a conversation group
        /// </summary>
        public async Task LeaveConversationGroup(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            
            var userId = Context.User?.FindFirst("sub")?.Value;
            _logger.LogInformation("User {UserId} left conversation group {ConversationId}", 
                userId, conversationId);
        }

        /// <summary>
        /// Notify group members that someone joined
        /// </summary>
        public async Task NotifyMemberJoined(string conversationId, string newMemberId, string memberName)
        {
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("MemberJoined", conversationId, newMemberId, memberName);
        }

        /// <summary>
        /// Notify group members that someone left
        /// </summary>
        public async Task NotifyMemberLeft(string conversationId, string memberId, string memberName)
        {
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("MemberLeft", conversationId, memberId, memberName);
        }

        /// <summary>
        /// Notify group members of settings changes (name, description, etc.)
        /// </summary>
        public async Task NotifyGroupUpdated(string conversationId, object updatedData)
        {
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("GroupUpdated", conversationId, updatedData);
        }
    }
}
