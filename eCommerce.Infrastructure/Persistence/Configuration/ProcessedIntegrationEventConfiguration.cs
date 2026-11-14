using eCommerce.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Persistence.Configuration;

public sealed class ProcessedIntegrationEventConfiguration : IEntityTypeConfiguration<ProcessedIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedIntegrationEvent> builder)
    {
        builder.HasKey(processed => new { processed.EventId, processed.ConsumerName });
        builder.Property(processed => processed.ConsumerName).HasMaxLength(200);
        builder.HasIndex(processed => processed.ProcessedAtUtc);
    }
}
