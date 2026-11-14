using eCommerce.Application.Common.Messaging.Events;
using eCommerce.Infrastructure.Messaging.AzureServiceBus;

namespace eCommerce.Messaging.Worker.Consumers;

public sealed class EmailNotificationAction(ILogger<EmailNotificationAction> logger) : IOrderCreatedAction
{
    public string ConsumerName => "email-notification";

    public Task HandleAsync(
        ServiceBusEvent<OrderCreatedPayload> integrationEvent,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Simulated order confirmation email for OrderId {OrderId}, OrderNumber {OrderNumber}, CorrelationId {CorrelationId}",
            integrationEvent.Payload.OrderId,
            integrationEvent.Payload.OrderNumber,
            integrationEvent.CorrelationId);
        return Task.CompletedTask;
    }
}
