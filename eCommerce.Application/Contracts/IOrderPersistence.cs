using eCommerce.Application.DTO.Request;

namespace eCommerce.Application.Contracts;

public interface IOrderPersistence
{
    Task<OrderCreationResult> CreateAsync(
        CreateOrderDto orderDto,
        string orderNumber,
        string? correlationId,
        CancellationToken cancellationToken = default);
}

public sealed record OrderCreationResult(int OrderId, string OrderNumber);
