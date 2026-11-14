using eCommerce.Application.DTO.Request;

namespace eCommerce.Application.Contracts
{
    public interface IOrderService
    {
        Task<OrderCreationResult> CreateOrderAsync(
            CreateOrderDto orderDto,
            string? correlationId,
            CancellationToken cancellationToken = default);
    }
}
