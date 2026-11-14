using eCommerce.Infrastructure.Messaging.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eCommerce.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    IOptions<ServiceBusOptions> serviceBusOptions,
    IOptions<OutboxProcessingOptions> outboxOptions,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    private readonly ServiceBusOptions _serviceBusOptions = serviceBusOptions.Value;
    private readonly OutboxProcessingOptions _outboxOptions = outboxOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Outbox publisher started. Enabled: {Enabled}, Topic: {TopicName}",
            _serviceBusOptions.Enabled,
            _serviceBusOptions.TopicName);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_serviceBusOptions.Enabled)
            {
                await Task.Delay(_outboxOptions.PollInterval, stoppingToken);
                continue;
            }

            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unexpected error while publishing outbox messages.");
                await Task.Delay(_outboxOptions.PollInterval, stoppingToken);
            }
        }

        logger.LogInformation("Outbox publisher stopped.");
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<OutboxMessageStore>();
        var publisher = scope.ServiceProvider.GetRequiredService<AzureServiceBusEventPublisher>();
        var messages = await store.ClaimBatchAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await Task.Delay(_outboxOptions.PollInterval, cancellationToken);
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishOutboxMessageAsync(message, cancellationToken);
                await store.MarkPublishedAsync(message.Id, cancellationToken);
                logger.LogInformation(
                    "Published outbox message {MessageId} with EventId {EventId}",
                    message.Id,
                    message.EventId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to publish outbox message {MessageId} with EventId {EventId}",
                    message.Id,
                    message.EventId);
                await store.MarkFailedAsync(message.Id, exception.Message, cancellationToken);
            }
        }
    }
}
