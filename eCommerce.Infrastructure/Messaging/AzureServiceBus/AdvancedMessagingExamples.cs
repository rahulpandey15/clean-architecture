using Azure.Messaging.ServiceBus;

namespace eCommerce.Infrastructure.Messaging.AzureServiceBus;

public sealed class AdvancedMessagingExamples(
    ServiceBusClientProvider clientProvider)
{
    public async Task SendToQueueAsync(
        string queueName,
        BinaryData body,
        CancellationToken cancellationToken = default)
    {
        await using var sender = clientProvider.GetClient().CreateSender(queueName);
        await sender.SendMessageAsync(new ServiceBusMessage(body), cancellationToken);
    }

    public async Task SendBatchAsync(
        string entityPath,
        IEnumerable<BinaryData> bodies,
        CancellationToken cancellationToken = default)
    {
        await using var sender = clientProvider.GetClient().CreateSender(entityPath);
        using var batch = await sender.CreateMessageBatchAsync(cancellationToken);
        foreach (var body in bodies)
        {
            if (!batch.TryAddMessage(new ServiceBusMessage(body)))
            {
                throw new InvalidOperationException("The message does not fit in the current Service Bus batch.");
            }
        }

        await sender.SendMessagesAsync(batch, cancellationToken);
    }

    public async Task<long> ScheduleMessageAsync(
        string entityPath,
        BinaryData body,
        DateTimeOffset scheduledEnqueueTimeUtc,
        CancellationToken cancellationToken = default)
    {
        await using var sender = clientProvider.GetClient().CreateSender(entityPath);
        return await sender.ScheduleMessageAsync(
            new ServiceBusMessage(body),
            scheduledEnqueueTimeUtc,
            cancellationToken);
    }

    public async Task SendWithTtlAsync(
        string entityPath,
        BinaryData body,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        await using var sender = clientProvider.GetClient().CreateSender(entityPath);
        await sender.SendMessageAsync(new ServiceBusMessage(body)
        {
            TimeToLive = ttl
        }, cancellationToken);
    }

    public ServiceBusSessionProcessor CreateSessionProcessor(
        string entityPath,
        int maxConcurrentSessions = 4)
    {
        return clientProvider.GetClient().CreateSessionProcessor(
            entityPath,
            new ServiceBusSessionProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentSessions = maxConcurrentSessions,
                MaxConcurrentCallsPerSession = 1
            });
    }

    public ServiceBusProcessor CreateCorrelationProcessor(
        string topicName,
        string subscriptionName)
    {
        return clientProvider.GetClient().CreateProcessor(
            topicName,
            subscriptionName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false
            });
    }

    public ServiceBusReceiver CreateDeadLetterReceiver(string entityPath)
    {
        return clientProvider.GetClient().CreateReceiver(
            entityPath,
            new ServiceBusReceiverOptions
            {
                SubQueue = SubQueue.DeadLetter
            });
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveDeferredAsync(
        string entityPath,
        IEnumerable<long> sequenceNumbers,
        CancellationToken cancellationToken = default)
    {
        await using var receiver = clientProvider.GetClient().CreateReceiver(entityPath);
        return await receiver.ReceiveDeferredMessagesAsync(
            sequenceNumbers,
            cancellationToken);
    }
}
