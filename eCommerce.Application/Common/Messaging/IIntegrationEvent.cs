namespace eCommerce.Application.Common.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    string EventType { get; }
    int EventVersion { get; }
    DateTimeOffset OccurredAtUtc { get; }
    string? CorrelationId { get; }
    string? CausationId { get; }
    object Payload { get; }
}
