using eCommerce.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Persistence.Configuration;

public sealed class OrderAuditEntryConfiguration : IEntityTypeConfiguration<OrderAuditEntry>
{
    public void Configure(EntityTypeBuilder<OrderAuditEntry> builder)
    {
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Action).IsRequired().HasMaxLength(200);
        builder.Property(entry => entry.Details).IsRequired().HasMaxLength(4000);
        builder.HasIndex(entry => new { entry.OrderId, entry.EventId }).IsUnique();
    }
}
