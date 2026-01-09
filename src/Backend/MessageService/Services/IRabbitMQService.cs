using MessengerContracts.DTOs;

namespace MessageService.Services;

/// <summary>
/// Service for publishing events to RabbitMQ.
/// </summary>
public interface IRabbitMQService
{
    /// <summary>
    /// Publishes a message sent event.
    /// </summary>
    Task PublishMessageSentEventAsync(MessageSentEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a message delivered event.
    /// </summary>
    Task PublishMessageDeliveredEventAsync(MessageDeliveredEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a message read event.
    /// </summary>
    Task PublishMessageReadEventAsync(MessageReadEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a user typing event.
    /// </summary>
    Task PublishUserTypingEventAsync(UserTypingEvent eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a user online status event.
    /// </summary>
    Task PublishUserOnlineEventAsync(UserOnlineEvent eventData, CancellationToken cancellationToken = default);
}
