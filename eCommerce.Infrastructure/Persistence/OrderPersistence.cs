using eCommerce.Application.Common.Messaging.Events;
using eCommerce.Application.Common.Messaging;
using eCommerce.Application.Contracts;
using eCommerce.Application.DTO.Request;
using eCommerce.Infrastructure.Messaging.Outbox;
using eCommerce.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Persistence;

public sealed class OrderPersistence(
    AppDbContext dbContext,
    IOutboxWriter outboxWriter) : IOrderPersistence
{
    public async Task<OrderCreationResult> CreateAsync(
        CreateOrderDto orderDto,
        string orderNumber,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var order = new Order
        {
            OrderNumber = orderNumber,
            TotalAmount = orderDto.Items.Sum(item => item.UnitPrice * item.Quantity),
            Status = "Pending",
            UserId = orderDto.UserId,
            CreatedBy = orderDto.UserId.ToString(),
            OrderItems = orderDto.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity,
                CreatedBy = orderDto.UserId.ToString()
            }).ToList()
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var integrationEvent = new OrderCreatedEvent
        {
            CorrelationId = correlationId,
            Payload = new OrderCreatedPayload
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Items = order.OrderItems.Select(item => new OrderCreatedItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToArray()
            }
        };

        await outboxWriter.AddAsync(integrationEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new OrderCreationResult(order.Id, order.OrderNumber);
    }
}
