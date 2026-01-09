using MessengerContracts.DTOs;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services;

/// <summary>
/// Background service that consumes events from RabbitMQ and forwards them to SignalR clients.
/// </summary>
public class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string ExchangeName = "messenger.events";
    private const string QueueName = "notification.queue";

    public RabbitMQConsumerService(
        IServiceProvider serviceProvider,
        ILogger<RabbitMQConsumerService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Consumer Service starting...");

        try
        {
            InitializeRabbitMQ();

            // Create scope for SignalR hub
            using IServiceScope scope = _serviceProvider.CreateScope();
            IHubContext<NotificationHub> hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

            // Setup consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    string routingKey = ea.RoutingKey;

                    _logger.LogDebug("Received message with routing key: {RoutingKey}", routingKey);

                    // Route based on routing key
                    await RouteEventAsync(routingKey, message, hubContext);

                    // Acknowledge message
                    _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from RabbitMQ");
                    // Reject message and don't requeue
                    _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            // Start consuming
            _channel?.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("RabbitMQ Consumer Service started successfully");

            // Keep running until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in RabbitMQ Consumer Service");
            throw;
        }
    }

    private void InitializeRabbitMQ()
    {
        string? hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        int port = int.TryParse(_configuration["RabbitMQ:Port"], out int p) ? p : 5672;
        string? userName = _configuration["RabbitMQ:UserName"] ?? "guest";
        string? password = _configuration["RabbitMQ:Password"] ?? "guest";

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange (should already exist from MessageService)
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        // Declare queue
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind queue to exchange with routing patterns
        _channel.QueueBind(QueueName, ExchangeName, "message.*");
        _channel.QueueBind(QueueName, ExchangeName, "user.*");
        _channel.QueueBind(QueueName, ExchangeName, "key.*");

        // Set QoS to process one message at a time
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation("RabbitMQ connection established to {HostName}:{Port}", hostName, port);
    }

    private async Task RouteEventAsync(string routingKey, string message, IHubContext<NotificationHub> hubContext)
    {
        switch (routingKey)
        {
            case "message.sent":
                await HandleMessageSentAsync(message, hubContext);
                break;

            case "message.delivered":
                await HandleMessageDeliveredAsync(message, hubContext);
                break;

            case "message.read":
                await HandleMessageReadAsync(message, hubContext);
                break;

            case "user.typing":
                await HandleUserTypingAsync(message, hubContext);
                break;

            case "user.online":
                await HandleUserOnlineAsync(message, hubContext);
                break;

            case "key.rotated":
                await HandleKeyRotationAsync(message, hubContext);
                break;

            default:
                _logger.LogWarning("Unknown routing key: {RoutingKey}", routingKey);
                break;
        }
    }

    private async Task HandleMessageSentAsync(string message, IHubContext<NotificationHub> hubContext)
    {
        MessageSentEvent? eventData = JsonSerializer.Deserialize<MessageSentEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData == null)
        {
            _logger.LogWarning("Failed to deserialize MessageSentEvent");
            return;
        }

        _logger.LogDebug("Forwarding MessageSent to {Count} recipients", eventData.RecipientIds.Count);

        // Send to each recipient via SignalR
        foreach (Guid recipientId in eventData.RecipientIds)
        {
            await hubContext.Clients.User(recipientId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    messageId = eventData.MessageId,
                    senderId = eventData.SenderId,
                    conversationId = eventData.ConversationId,
                    timestamp = eventData.Timestamp,
                    messageType = eventData.MessageType
                });
        }

        _logger.LogInformation("MessageSent notification sent for message {MessageId}", eventData.MessageId);
    }

    private async Task HandleMessageDeliveredAsync(string message, IHubContext<NotificationHub> hubContext)
    {
        MessageDeliveredEvent? eventData = JsonSerializer.Deserialize<MessageDeliveredEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData == null) return;

        await hubContext.Clients.User(eventData.RecipientId.ToString())
            .SendAsync("MessageDelivered", new
            {
                messageId = eventData.MessageId,
                deliveredAt = eventData.DeliveredAt
            });

        _logger.LogDebug("MessageDelivered notification sent for message {MessageId}", eventData.MessageId);
    }

    private async Task HandleMessageReadAsync(string message, IHubContext<NotificationHub> hubContext)
    {
        MessageReadEvent? eventData = JsonSerializer.Deserialize<MessageReadEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData == null) return;

        await hubContext.Clients.User(eventData.ReadBy.ToString())
            .SendAsync("MessageRead", new
            {
                messageId = eventData.MessageId,
                readBy = eventData.ReadBy,
                readAt = eventData.ReadAt
            });

        _logger.LogDebug("MessageRead notification sent for message {MessageId}", eventData.MessageId);
    }

    private async Task HandleUserTypingAsync(string message, IHubContext<NotificationHub> hubContext)
    {
        UserTypingEvent? eventData = JsonSerializer.Deserialize<UserTypingEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData == null) return;

        // Notify all users in the conversation except the sender
        await hubContext.Clients.Group($"conversation_{eventData.ConversationId}")
            .SendAsync("UserTyping", new
            {
                userId = eventData.UserId,
                conversationId = eventData.ConversationId,
                isTyping = eventData.IsTyping
            });

        _logger.LogDebug("UserTyping notification sent for conversation {ConversationId}", eventData.ConversationId);
    }

    private async Task HandleUserOnlineAsync(string message, IHubContext<NotificationHub> hubContext)
    {
        UserOnlineEvent? eventData = JsonSerializer.Deserialize<UserOnlineEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData == null) return;

        // Broadcast to all connected clients
        await hubContext.Clients.All.SendAsync("UserOnlineStatus", new
        {
            userId = eventData.UserId,
            isOnline = eventData.IsOnline,
            timestamp = eventData.Timestamp
        });

        _logger.LogDebug("UserOnlineStatus notification sent for user {UserId}", eventData.UserId);
    }

    private async Task HandleKeyRotationAsync(string message, IHubContext<NotificationHub> hubContext)
    {
        KeyRotationEvent? eventData = JsonSerializer.Deserialize<KeyRotationEvent>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (eventData == null) return;

        // Notify the user whose key was rotated
        await hubContext.Clients.User(eventData.UserId.ToString())
            .SendAsync("KeyRotated", new
            {
                userId = eventData.UserId,
                newKeyId = eventData.NewKeyId,
                oldKeyId = eventData.OldKeyId,
                timestamp = eventData.Timestamp
            });

        _logger.LogInformation("KeyRotation notification sent for user {UserId}", eventData.UserId);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        _logger.LogInformation("RabbitMQ Consumer Service disposed");

        base.Dispose();
    }
}
