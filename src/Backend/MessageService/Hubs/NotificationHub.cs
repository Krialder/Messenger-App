using Microsoft.AspNetCore.SignalR;

namespace MessageService.Hubs
{
    public class NotificationHub : Hub
    {
        // PSEUDO CODE: SignalR Hub for Real-time Notifications
        // Handles message delivery, typing indicators, online status

        public override async Task OnConnectedAsync()
        {
            // PSEUDO CODE:
            // 1. Get user ID from JWT claims
            // 2. Add connection to user's group: Groups.AddToGroupAsync(connectionId, userId)
            // 3. Update online status in Redis (SET user:{userId}:online TRUE EX 300)
            // 4. Broadcast online status to user's contacts
            
            var userId = Context.User?.FindFirst("sub")?.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, userId!);
            
            await Clients.Others.SendAsync("UserOnline", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // PSEUDO CODE:
            // 1. Get user ID from claims
            // 2. Remove from group
            // 3. Update Redis: DEL user:{userId}:online
            // 4. Broadcast offline status
            
            var userId = Context.User?.FindFirst("sub")?.Value;
            await Clients.Others.SendAsync("UserOffline", userId);
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTypingIndicator(string recipientId)
        {
            // PSEUDO CODE:
            // 1. Get sender ID from claims
            // 2. Send typing indicator to specific recipient
            //    - Throttled: max 1 per 3 seconds
            // 3. Auto-expire after 5 seconds
            
            await Clients.Group(recipientId).SendAsync("TypingIndicator", Context.User?.FindFirst("sub")?.Value);
        }

        public async Task StopTypingIndicator(string recipientId)
        {
            // PSEUDO CODE:
            // 1. Send stop typing event to recipient
            
            await Clients.Group(recipientId).SendAsync("StopTyping", Context.User?.FindFirst("sub")?.Value);
        }

        // Called by MessageService when new message arrives
        public async Task NotifyNewMessage(string recipientId, object messageData)
        {
            // PSEUDO CODE:
            // 1. Send push notification to recipient's active connections
            // 2. Client will decrypt message locally
            
            await Clients.Group(recipientId).SendAsync("NewMessage", messageData);
        }

        public async Task NotifyMessageRead(string senderId, string messageId)
        {
            // PSEUDO CODE:
            // 1. Notify sender that their message was read
            // 2. Update UI to show double checkmark
            
            await Clients.Group(senderId).SendAsync("MessageRead", messageId);
        }
    }
}
