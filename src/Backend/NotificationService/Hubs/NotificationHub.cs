using Microsoft.AspNetCore.SignalR;
using MessengerCommon.Constants;

namespace NotificationService.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// Handles: MessageReceived, MessageRead, TypingIndicators, PresenceManagement
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly IPresenceService _presenceService;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(IPresenceService presenceService, ILogger<NotificationHub> logger)
        {
            _presenceService = presenceService;
            _logger = logger;
        }

        // CONNECTION LIFECYCLE
        
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await _presenceService.SetUserOnlineAsync(userId);
                await Clients.Others.SendAsync(SignalREvents.USER_ONLINE, userId);
                _logger.LogInformation("User {UserId} connected", userId);
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await _presenceService.SetUserOfflineAsync(userId);
                await Clients.Others.SendAsync(SignalREvents.USER_OFFLINE, userId);
                _logger.LogInformation("User {UserId} disconnected", userId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // MESSAGING EVENTS

        /// <summary>
        /// Notify recipient that a new message was received
        /// </summary>
        public async Task NotifyMessageReceived(string recipientId, object message)
        {
            await Clients.User(recipientId).SendAsync(SignalREvents.MESSAGE_RECEIVED, message);
            _logger.LogDebug("Notified user {RecipientId} of new message", recipientId);
        }

        /// <summary>
        /// Notify sender that message was read
        /// </summary>
        public async Task NotifyMessageRead(string senderId, Guid messageId)
        {
            await Clients.User(senderId).SendAsync(SignalREvents.MESSAGE_READ, messageId);
            _logger.LogDebug("Notified user {SenderId} that message {MessageId} was read", senderId, messageId);
        }

        // TYPING INDICATORS

        /// <summary>
        /// Notify that user started typing
        /// </summary>
        public async Task NotifyTypingStarted(string recipientId)
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.User(recipientId).SendAsync(SignalREvents.TYPING_STARTED, userId);
            }
        }

        /// <summary>
        /// Notify that user stopped typing
        /// </summary>
        public async Task NotifyTypingStopped(string recipientId)
        {
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.User(recipientId).SendAsync(SignalREvents.TYPING_STOPPED, userId);
            }
        }

        // PRESENCE MANAGEMENT

        /// <summary>
        /// Get online status of users
        /// </summary>
        public async Task<Dictionary<string, bool>> GetOnlineStatus(List<string> userIds)
        {
            return await _presenceService.GetOnlineStatusAsync(userIds);
        }
    }

    /// <summary>
    /// Interface for presence management service
    /// </summary>
    public interface IPresenceService
    {
        Task SetUserOnlineAsync(string userId);
        Task SetUserOfflineAsync(string userId);
        Task<bool> IsUserOnlineAsync(string userId);
        Task<Dictionary<string, bool>> GetOnlineStatusAsync(List<string> userIds);
    }
}
