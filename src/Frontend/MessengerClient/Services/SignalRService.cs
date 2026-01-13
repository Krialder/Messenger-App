using Microsoft.AspNetCore.SignalR.Client;
using MessengerContracts.DTOs;

namespace MessengerClient.Services
{
    /// <summary>
    /// SignalR service for real-time messaging and notifications
    /// Manages WebSocket connection to NotificationHub with automatic reconnection
    /// </summary>
    public class SignalRService
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;

        public event Func<MessageDto, Task>? OnMessageReceived;
        public event Func<Guid, Task>? OnTypingIndicator;
        public event Func<Guid, Task>? OnUserOnline;
        public event Func<Guid, Task>? OnUserOffline;
        public event Func<Guid, Guid, Task>? OnMessageRead;
        public event Func<string, Task>? OnConnectionError;

        public SignalRService(string gatewayUrl)
        {
            _hubUrl = $"{gatewayUrl}/hubs/notifications";
        }

        /// <summary>
        /// Establish SignalR connection with JWT authentication
        /// Returns true if connection successful, false otherwise
        /// </summary>
        public async Task<bool> ConnectAsync(string jwtToken)
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
                    })
                    .WithAutomaticReconnect(new[]
                    {
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    })
                    .Build();

                _hubConnection.On<MessageDto>("NewMessage", async message =>
                {
                    if (OnMessageReceived != null)
                        await OnMessageReceived.Invoke(message);
                });

                _hubConnection.On<Guid>("TypingIndicator", async userId =>
                {
                    if (OnTypingIndicator != null)
                        await OnTypingIndicator.Invoke(userId);
                });

                _hubConnection.On<Guid>("UserOnline", async userId =>
                {
                    if (OnUserOnline != null)
                        await OnUserOnline.Invoke(userId);
                });

                _hubConnection.On<Guid>("UserOffline", async userId =>
                {
                    if (OnUserOffline != null)
                        await OnUserOffline.Invoke(userId);
                });

                _hubConnection.On<Guid, Guid>("MessageRead", async (senderId, messageId) =>
                {
                    if (OnMessageRead != null)
                        await OnMessageRead.Invoke(senderId, messageId);
                });

                _hubConnection.Reconnecting += error =>
                {
                    Console.WriteLine($"Connection lost, reconnecting... {error?.Message}");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += connectionId =>
                {
                    Console.WriteLine($"Reconnected successfully: {connectionId}");
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += async error =>
                {
                    Console.WriteLine($"Connection closed: {error?.Message}");
                    if (OnConnectionError != null)
                        await OnConnectionError.Invoke(error?.Message ?? "Connection closed");
                };

                await _hubConnection.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR connection failed: {ex.Message}");
                if (OnConnectionError != null)
                    await OnConnectionError.Invoke(ex.Message);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public async Task SendTypingIndicatorAsync(Guid recipientId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendTypingIndicator", recipientId);
            }
        }

        public async Task MarkMessageAsReadAsync(Guid messageId, Guid senderId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("MarkMessageAsRead", messageId, senderId);
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    }
}
