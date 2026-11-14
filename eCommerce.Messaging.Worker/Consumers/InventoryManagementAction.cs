using eCommerce.Application.Common.Messaging.Events;
using eCommerce.Infrastructure.Messaging.AzureServiceBus;

namespace eCommerce.Messaging.Worker.Consumers;

public sealed class InventoryManagementAction(ILogger<InventoryManagementAction> logger) : IOrderCreatedAction
{
    public string ConsumerName => "inventory-management";

    public Task HandleAsync(
        ServiceBusEvent<OrderCreatedPayload> integrationEvent,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Simulated inventory reservation for OrderId {OrderId} with {ItemCount} item types, CorrelationId {CorrelationId}",
            integrationEvent.Payload.OrderId,
            integrationEvent.Payload.Items.Count,
            integrationEvent.CorrelationId);
        return Task.CompletedTask;
    }
}
