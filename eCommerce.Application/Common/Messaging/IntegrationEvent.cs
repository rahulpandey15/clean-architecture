namespace eCommerce.Application.Common.Messaging;

public abstract record IntegrationEvent<TPayload> : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public abstract string EventType { get; }
    public virtual int EventVersion => 1;
    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public required TPayload Payload { get; init; }

    object IIntegrationEvent.Payload => Payload!;
}
