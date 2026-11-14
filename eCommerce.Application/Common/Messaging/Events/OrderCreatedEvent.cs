using eCommerce.Application.Common.Messaging;

namespace eCommerce.Application.Common.Messaging.Events;

public sealed record OrderCreatedEvent : IntegrationEvent<OrderCreatedPayload>
{
    public override string EventType => "OrderCreated";
}

public sealed record OrderCreatedPayload
{
    public required int OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required int UserId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string Status { get; init; }
    public required IReadOnlyCollection<OrderCreatedItem> Items { get; init; }
}

public sealed record OrderCreatedItem
{
    public required int ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal TotalPrice { get; init; }
}
