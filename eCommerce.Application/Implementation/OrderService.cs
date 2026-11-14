using eCommerce.Application.Contracts;
using eCommerce.Application.DTO.Request;

namespace eCommerce.Application.Implementation
{
    public sealed class OrderService(IOrderPersistence orderPersistence) : IOrderService
    {
        public Task<OrderCreationResult> CreateOrderAsync(
            CreateOrderDto orderDto,
            string? correlationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(orderDto);
            if (orderDto.UserId <= 0)
            {
                throw new ArgumentException("A valid user is required.", nameof(orderDto));
            }

            if (orderDto.Items.Count == 0)
            {
                throw new ArgumentException("An order must contain at least one item.", nameof(orderDto));
            }

            if (orderDto.Items.Any(item => item.ProductId <= 0 || item.Quantity <= 0 || item.UnitPrice < 0))
            {
                throw new ArgumentException("Order items must contain valid product, quantity, and price values.", nameof(orderDto));
            }

            var orderNumber = $"ORD-{DateTime.UtcNow.Ticks}";
            return orderPersistence.CreateAsync(orderDto, orderNumber, correlationId, cancellationToken);
        }
    }
}
