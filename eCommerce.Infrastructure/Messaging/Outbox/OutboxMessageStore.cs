using System.Data;
using eCommerce.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace eCommerce.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessingOptions
{
    public int BatchSize { get; set; } = 25;
    public int MaxAttempts { get; set; } = 8;
    public TimeSpan LockDuration { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
}

public sealed class OutboxMessageStore(
    AppDbContext dbContext,
    IOptions<OutboxProcessingOptions> options)
{
    private readonly OutboxProcessingOptions _options = options.Value;

    public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        var messages = await dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT TOP ({_options.BatchSize}) *
                FROM [OutboxMessages] WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE [PublishedAtUtc] IS NULL
                  AND [DeadLetteredAtUtc] IS NULL
                  AND [NextAttemptAtUtc] <= {now}
                  AND ([LockedUntilUtc] IS NULL OR [LockedUntilUtc] < {now})
                ORDER BY [Id]
                """)
            .ToListAsync(cancellationToken);

        var lockUntil = now.Add(_options.LockDuration);
        foreach (var message in messages)
        {
            message.LockedUntilUtc = lockUntil;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return messages;
    }

    public async Task MarkPublishedAsync(
        long messageId,
        CancellationToken cancellationToken)
    {
        await dbContext.OutboxMessages
            .Where(message => message.Id == messageId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(message => message.PublishedAtUtc, DateTimeOffset.UtcNow)
                .SetProperty(message => message.LockedUntilUtc, (DateTimeOffset?)null),
                cancellationToken);
    }

    public async Task MarkFailedAsync(
        long messageId,
        string error,
        CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages
            .SingleAsync(item => item.Id == messageId, cancellationToken);

        message.AttemptCount++;
        message.LastError = error.Length > 4000 ? error[..4000] : error;
        message.LockedUntilUtc = null;

        if (message.AttemptCount >= _options.MaxAttempts)
        {
            message.DeadLetteredAtUtc = DateTimeOffset.UtcNow;
        }
        else
        {
            var delay = TimeSpan.FromSeconds(Math.Min(
                Math.Pow(2, message.AttemptCount),
                TimeSpan.FromHours(1).TotalSeconds));
            message.NextAttemptAtUtc = DateTimeOffset.UtcNow.Add(delay);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
