using System.Text.Json;
using eCommerce.Application.Common.Messaging;
using eCommerce.Infrastructure.Persistence.Entities;

namespace eCommerce.Infrastructure.Messaging.Outbox;

public sealed class OutboxWriter(AppDbContext dbContext) : IOutboxWriter
{
    public Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var payload = JsonSerializer.Serialize(
            integrationEvent.Payload,
            integrationEvent.Payload.GetType());

        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            EventId = integrationEvent.EventId,
            EventType = integrationEvent.EventType,
            EventVersion = integrationEvent.EventVersion,
            OccurredAtUtc = integrationEvent.OccurredAtUtc,
            CorrelationId = integrationEvent.CorrelationId,
            CausationId = integrationEvent.CausationId,
            Payload = payload,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            NextAttemptAtUtc = DateTimeOffset.UtcNow
        });

        return Task.CompletedTask;
    }
}
