namespace eCommerce.Infrastructure.Persistence.Entities;

public sealed class OrderAuditEntry : AuditableEntity
{
    public int OrderId { get; set; }
    public Guid EventId { get; set; }
    public required string Action { get; set; }
    public required string Details { get; set; }
}
