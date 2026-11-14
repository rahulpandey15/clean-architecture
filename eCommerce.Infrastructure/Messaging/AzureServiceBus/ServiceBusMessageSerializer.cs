using System.Text.Json;
using eCommerce.Application.Common.Messaging;
using eCommerce.Infrastructure.Persistence.Entities;

namespace eCommerce.Infrastructure.Messaging.AzureServiceBus;

public sealed class ServiceBusMessageSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public BinaryData Serialize(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        return SerializeEnvelope(
            integrationEvent.EventId,
            integrationEvent.EventType,
            integrationEvent.EventVersion,
            integrationEvent.OccurredAtUtc,
            integrationEvent.CorrelationId,
            integrationEvent.CausationId,
            integrationEvent.Payload);
    }

    public BinaryData Serialize(OutboxMessage message)
    {
        using var document = JsonDocument.Parse(message.Payload);
        return SerializeEnvelope(
            message.EventId,
            message.EventType,
            message.EventVersion,
            message.OccurredAtUtc,
            message.CorrelationId,
            message.CausationId,
            document.RootElement);
    }

    public TPayload DeserializePayload<TPayload>(BinaryData body)
    {
        return DeserializeEnvelope<TPayload>(body).Payload;
    }

    public ServiceBusEvent<TPayload> DeserializeEnvelope<TPayload>(BinaryData body)
    {
        var envelope = JsonSerializer.Deserialize<ServiceBusEvent<TPayload>>(body, JsonOptions)
            ?? throw new JsonException("Integration event envelope is empty.");

        if (envelope.EventId == Guid.Empty || string.IsNullOrWhiteSpace(envelope.EventType))
        {
            throw new JsonException("Integration event metadata is incomplete.");
        }

        return envelope;
    }

    private static BinaryData SerializeEnvelope(
        Guid eventId,
        string eventType,
        int eventVersion,
        DateTimeOffset occurredAtUtc,
        string? correlationId,
        string? causationId,
        object payload)
    {
        var envelope = new
        {
            eventId,
            eventType,
            eventVersion,
            occurredAtUtc,
            correlationId,
            causationId,
            payload
        };

        return BinaryData.FromObjectAsJson(envelope, JsonOptions);
    }
}

public sealed record ServiceBusEvent<TPayload>
{
    public Guid EventId { get; init; }
    public required string EventType { get; init; }
    public int EventVersion { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public required TPayload Payload { get; init; }
}
