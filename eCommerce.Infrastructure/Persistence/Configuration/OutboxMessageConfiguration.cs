using eCommerce.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Persistence.Configuration;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(message => message.Id);
        builder.Property(message => message.EventType).IsRequired().HasMaxLength(200);
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.CorrelationId).HasMaxLength(200);
        builder.Property(message => message.CausationId).HasMaxLength(200);
        builder.Property(message => message.LastError).HasMaxLength(4000);
        builder.HasIndex(message => message.EventId).IsUnique();
        builder.HasIndex(message => new
        {
            message.PublishedAtUtc,
            message.DeadLetteredAtUtc,
            message.NextAttemptAtUtc
        });
    }
}
