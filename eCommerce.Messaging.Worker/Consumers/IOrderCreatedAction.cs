using eCommerce.Application.Common.Messaging.Events;
using eCommerce.Infrastructure.Messaging.AzureServiceBus;

namespace eCommerce.Messaging.Worker.Consumers;

public interface IOrderCreatedAction
{
    string ConsumerName { get; }
    Task HandleAsync(
        ServiceBusEvent<OrderCreatedPayload> integrationEvent,
        CancellationToken cancellationToken);
}
