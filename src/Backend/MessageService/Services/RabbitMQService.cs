using MessengerContracts.DTOs;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MessageService.Services;

/// <summary>
/// RabbitMQ service for publishing events to message queue.
/// </summary>
public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private const string ExchangeName = "messenger.events";
    private bool _disposed;

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _logger = logger;

        string? hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        int port = int.TryParse(configuration["RabbitMQ:Port"], out int p) ? p : 5672;
        string? userName = configuration["RabbitMQ:UserName"] ?? "guest";
        string? password = configuration["RabbitMQ:Password"] ?? "guest";

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            DispatchConsumersAsync = true
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare topic exchange for event routing
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ connection established to {HostName}:{Port}", hostName, port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {HostName}:{Port}", hostName, port);
            throw;
        }
    }

    public Task PublishMessageSentEventAsync(MessageSentEvent eventData, CancellationToken cancellationToken = default)
    {
        return PublishEventAsync("message.sent", eventData, cancellationToken);
    }

    public Task PublishMessageDeliveredEventAsync(MessageDeliveredEvent eventData, CancellationToken cancellationToken = default)
    {
        return PublishEventAsync("message.delivered", eventData, cancellationToken);
    }

    public Task PublishMessageReadEventAsync(MessageReadEvent eventData, CancellationToken cancellationToken = default)
    {
        return PublishEventAsync("message.read", eventData, cancellationToken);
    }

    public Task PublishUserTypingEventAsync(UserTypingEvent eventData, CancellationToken cancellationToken = default)
    {
        return PublishEventAsync("user.typing", eventData, cancellationToken);
    }

    public Task PublishUserOnlineEventAsync(UserOnlineEvent eventData, CancellationToken cancellationToken = default)
    {
        return PublishEventAsync("user.online", eventData, cancellationToken);
    }

    private Task PublishEventAsync<T>(string routingKey, T eventData, CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMQService));
        }

        try
        {
            string json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            byte[] body = Encoding.UTF8.GetBytes(json);

            IBasicProperties properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Published event {RoutingKey} to RabbitMQ", routingKey);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {RoutingKey} to RabbitMQ", routingKey);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        _disposed = true;

        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
