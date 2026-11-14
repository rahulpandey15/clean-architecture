using Azure.Messaging.ServiceBus;
using System.Diagnostics;
using eCommerce.Application.Common.Messaging;
using eCommerce.Infrastructure.Persistence.Entities;
using eCommerce.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eCommerce.Infrastructure.Messaging.AzureServiceBus;

public sealed class AzureServiceBusEventPublisher(
    ServiceBusClientProvider clientProvider,
    ServiceBusMessageSerializer serializer,
    IOptions<ServiceBusOptions> options,
    ILogger<AzureServiceBusEventPublisher> logger) : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusOptions _options = options.Value;
    private ServiceBusSender? _sender;

    public Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        return SendAsync(
            integrationEvent.EventId,
            integrationEvent.EventType,
            integrationEvent.EventVersion,
            integrationEvent.CorrelationId,
            serializer.Serialize(integrationEvent),
            cancellationToken);
    }

    public Task PublishOutboxMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            message.EventId,
            message.EventType,
            message.EventVersion,
            message.CorrelationId,
            serializer.Serialize(message),
            cancellationToken);
    }

    private async Task SendAsync(
        Guid eventId,
        string eventType,
        int eventVersion,
        string? correlationId,
        BinaryData body,
        CancellationToken cancellationToken)
    {
        using var activity = MessagingTelemetry.ActivitySource.StartActivity(
            "messaging.publish",
            ActivityKind.Producer);
        activity?.SetTag("messaging.system", "azure-service-bus");
        activity?.SetTag("messaging.destination.name", _options.TopicName);
        activity?.SetTag("messaging.event.type", eventType);
        activity?.SetTag("messaging.event.id", eventId);
        activity?.SetTag("messaging.correlation_id", correlationId);

        var serviceBusMessage = new ServiceBusMessage(body)
        {
            MessageId = eventId.ToString("D"),
            Subject = eventType,
            ContentType = "application/json",
            CorrelationId = correlationId
        };
        serviceBusMessage.ApplicationProperties["EventType"] = eventType;
        serviceBusMessage.ApplicationProperties["EventVersion"] = eventVersion;
        serviceBusMessage.ApplicationProperties["EventId"] = eventId.ToString("D");

        await (await GetSenderAsync(cancellationToken)).SendMessageAsync(
            serviceBusMessage,
            cancellationToken);

        logger.LogInformation(
            "Published integration event {EventType} with EventId {EventId} and CorrelationId {CorrelationId} to topic {TopicName}",
            eventType,
            eventId,
            correlationId,
            _options.TopicName);
        MessagingTelemetry.PublishedEvents.Add(1,
            new KeyValuePair<string, object?>("event.type", eventType));
    }

    private ValueTask<ServiceBusSender> GetSenderAsync(CancellationToken cancellationToken)
    {
        _sender ??= clientProvider.GetClient().CreateSender(_options.TopicName);
        return ValueTask.FromResult(_sender);
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender is not null)
        {
            await _sender.DisposeAsync();
        }
    }
}
