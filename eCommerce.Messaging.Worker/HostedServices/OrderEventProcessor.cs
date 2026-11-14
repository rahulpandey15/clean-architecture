using Azure.Messaging.ServiceBus;
using System.Diagnostics;
using System.Text.Json;
using eCommerce.Application.Common.Messaging.Events;
using eCommerce.Infrastructure.Messaging.AzureServiceBus;
using eCommerce.Infrastructure.Messaging;
using eCommerce.Infrastructure.Persistence.Entities;
using eCommerce.Messaging.Worker.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace eCommerce.Messaging.Worker.HostedServices;

public sealed class OrderEventProcessor(
    ServiceBusClientProvider clientProvider,
    ServiceBusMessageSerializer serializer,
    IServiceScopeFactory scopeFactory,
    IOptions<ServiceBusOptions> options,
    ILogger<OrderEventProcessor> logger) : IHostedService, IAsyncDisposable
{
    private readonly List<ServiceBusProcessor> _processors = [];
    private readonly ServiceBusOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            logger.LogWarning("Messaging worker is disabled because AzureServiceBus:Enabled is false.");
            return;
        }

        foreach (var subscriptionName in new[]
        {
            "email-notification",
            "inventory-management",
            "order-audit"
        })
        {
            var processor = clientProvider.GetClient().CreateProcessor(
                _options.TopicName,
                subscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = _options.MaxConcurrentCalls,
                    PrefetchCount = _options.PrefetchCount,
                    MaxAutoLockRenewalDuration = _options.MaxAutoLockRenewalDuration
                });

            processor.ProcessMessageAsync += args => ProcessMessageAsync(args, subscriptionName);
            processor.ProcessErrorAsync += args => ProcessErrorAsync(args, subscriptionName);
            _processors.Add(processor);
            await processor.StartProcessingAsync(cancellationToken);
            logger.LogInformation(
                "Started Service Bus processor for topic {TopicName}, subscription {SubscriptionName}",
                _options.TopicName,
                subscriptionName);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors)
        {
            await processor.StopProcessingAsync(cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args,
        string subscriptionName)
    {
        var message = args.Message;
        ServiceBusEvent<OrderCreatedPayload> integrationEvent;
        try
        {
            integrationEvent = serializer.DeserializeEnvelope<OrderCreatedPayload>(message.Body);
            if (!string.Equals(integrationEvent.EventType, "OrderCreated", StringComparison.Ordinal))
            {
                await args.DeadLetterMessageAsync(
                    message,
                    "UnsupportedEventType",
                    $"Event type '{integrationEvent.EventType}' is not supported.");
                return;
            }
        }
        catch (JsonException exception)
        {
            logger.LogError(
                exception,
                "Dead-lettering malformed message {MessageId} from subscription {SubscriptionName}",
                message.MessageId,
                subscriptionName);
            await args.DeadLetterMessageAsync(message, "MalformedMessage", exception.Message);
            return;
        }

        try
        {
            using var activity = MessagingTelemetry.ActivitySource.StartActivity(
                "messaging.process",
                ActivityKind.Consumer);
            activity?.SetTag("messaging.system", "azure-service-bus");
            activity?.SetTag("messaging.destination.name", _options.TopicName);
            activity?.SetTag("messaging.message.id", message.MessageId);
            activity?.SetTag("messaging.event.id", integrationEvent.EventId);
            activity?.SetTag("messaging.correlation_id", integrationEvent.CorrelationId);
            activity?.SetTag("messaging.destination.subscription", subscriptionName);

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var action = scope.ServiceProvider
                .GetRequiredService<IEnumerable<IOrderCreatedAction>>()
                .Single(item => item.ConsumerName == subscriptionName);

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var alreadyProcessed = await dbContext.ProcessedIntegrationEvents.AnyAsync(
                item => item.EventId == integrationEvent.EventId && item.ConsumerName == subscriptionName);
            if (alreadyProcessed)
            {
                await transaction.CommitAsync();
                await args.CompleteMessageAsync(message);
                logger.LogInformation(
                    "Completed duplicate message {MessageId} for EventId {EventId}, SubscriptionName {SubscriptionName}",
                    message.MessageId,
                    integrationEvent.EventId,
                    subscriptionName);
            MessagingTelemetry.ProcessedEvents.Add(1,
                new KeyValuePair<string, object?>("subscription", subscriptionName));
                return;
            }

            await action.HandleAsync(integrationEvent, CancellationToken.None);
            dbContext.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent
            {
                EventId = integrationEvent.EventId,
                ConsumerName = subscriptionName,
                ProcessedAtUtc = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            await args.CompleteMessageAsync(message);

            logger.LogInformation(
                "Processed message {MessageId} for EventId {EventId}, OrderId {OrderId}, DeliveryCount {DeliveryCount}, SubscriptionName {SubscriptionName}",
                message.MessageId,
                integrationEvent.EventId,
                integrationEvent.Payload.OrderId,
                message.DeliveryCount,
                subscriptionName);
        }
        catch (ServiceBusException exception) when (exception.Reason == ServiceBusFailureReason.MessageLockLost)
        {
            logger.LogWarning(
                exception,
                "Message lock expired for MessageId {MessageId}, EventId {EventId}, SubscriptionName {SubscriptionName}",
                message.MessageId,
                integrationEvent.EventId,
                subscriptionName);
        }
        catch (Exception exception)
        {
            MessagingTelemetry.FailedEvents.Add(1,
                new KeyValuePair<string, object?>("subscription", subscriptionName));
            logger.LogError(
                exception,
                "Transient processing failure for MessageId {MessageId}, EventId {EventId}, OrderId {OrderId}, DeliveryCount {DeliveryCount}, SubscriptionName {SubscriptionName}",
                message.MessageId,
                integrationEvent.EventId,
                integrationEvent.Payload.OrderId,
                message.DeliveryCount,
                subscriptionName);
            await args.AbandonMessageAsync(message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args, string subscriptionName)
    {
        logger.LogError(
            args.Exception,
            "Service Bus processor error for Entity {EntityPath}, SubscriptionName {SubscriptionName}, ErrorSource {ErrorSource}",
            args.EntityPath,
            subscriptionName,
            args.ErrorSource);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var processor in _processors)
        {
            await processor.DisposeAsync();
        }
    }
}
