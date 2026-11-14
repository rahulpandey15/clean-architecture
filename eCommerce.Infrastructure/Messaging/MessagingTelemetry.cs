using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace eCommerce.Infrastructure.Messaging;

public static class MessagingTelemetry
{
    public const string ActivitySourceName = "eCommerce.Messaging";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(ActivitySourceName);
    public static readonly Counter<long> PublishedEvents = Meter.CreateCounter<long>("ecommerce.messaging.events.published");
    public static readonly Counter<long> ProcessedEvents = Meter.CreateCounter<long>("ecommerce.messaging.events.processed");
    public static readonly Counter<long> FailedEvents = Meter.CreateCounter<long>("ecommerce.messaging.events.failed");
    public static readonly Histogram<double> ProcessingDuration = Meter.CreateHistogram<double>("ecommerce.messaging.processing.duration", "ms");
}
