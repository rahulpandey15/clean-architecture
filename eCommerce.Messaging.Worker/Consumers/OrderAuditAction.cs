using System.Text.Json;
using eCommerce.Application.Common.Messaging.Events;
using eCommerce.Infrastructure.Messaging.AzureServiceBus;
using eCommerce.Infrastructure.Persistence.Entities;

namespace eCommerce.Messaging.Worker.Consumers;

public sealed class OrderAuditAction(
    AppDbContext dbContext,
    ILogger<OrderAuditAction> logger) : IOrderCreatedAction
{
    public string ConsumerName => "order-audit";

    public Task HandleAsync(
        ServiceBusEvent<OrderCreatedPayload> integrationEvent,
        CancellationToken cancellationToken)
    {
        dbContext.OrderAuditEntries.Add(new OrderAuditEntry
        {
            OrderId = integrationEvent.Payload.OrderId,
            EventId = integrationEvent.EventId,
            Action = "OrderCreated",
            Details = JsonSerializer.Serialize(integrationEvent.Payload),
            CreatedBy = ConsumerName
        });

        logger.LogInformation(
            "Recorded audit entry for OrderId {OrderId}, EventId {EventId}, CorrelationId {CorrelationId}",
            integrationEvent.Payload.OrderId,
            integrationEvent.EventId,
            integrationEvent.CorrelationId);
        return Task.CompletedTask;
    }
}
