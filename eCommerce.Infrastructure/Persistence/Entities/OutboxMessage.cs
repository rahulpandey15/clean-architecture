namespace eCommerce.Infrastructure.Persistence.Entities;

public sealed class OutboxMessage
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public required string EventType { get; set; }
    public int EventVersion { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public required string Payload { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset NextAttemptAtUtc { get; set; }
    public DateTimeOffset? LockedUntilUtc { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
    public DateTimeOffset? DeadLetteredAtUtc { get; set; }
    public string? LastError { get; set; }
}
