namespace eCommerce.Infrastructure.Messaging.AzureServiceBus;

public sealed class ServiceBusOptions
{
    public bool Enabled { get; set; }
    public string? ConnectionString { get; set; }
    public string? FullyQualifiedNamespace { get; set; }
    public string TopicName { get; set; } = "order-events";
    public int MaxConcurrentCalls { get; set; } = 8;
    public int PrefetchCount { get; set; } = 20;
    public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxDeliveryCount { get; set; } = 10;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    public int RetryMaxRetries { get; set; } = 5;
}
