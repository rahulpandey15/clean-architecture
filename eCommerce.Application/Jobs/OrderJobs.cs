using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Jobs;

public class OrderJobs
{
    private readonly ILogger<OrderJobs> _logger;

    public OrderJobs(ILogger<OrderJobs> logger) => _logger = logger;

    public async Task SendOrderConfirmationAsync(Guid orderId)
    {
        _logger.LogInformation("Sending confirmation for order {OrderId}", orderId);
        await Task.Delay(500); // simulate email call
    }

    public async Task CancelUnpaidOrderAsync(Guid orderId)
    {
        _logger.LogInformation("Checking payment status for order {OrderId}", orderId);
        await Task.Delay(200);
    }

    public async Task CleanupAbandonedCartsAsync()
    {
        _logger.LogInformation("Cleaning abandoned carts at {Time}", DateTime.UtcNow);
        await Task.Delay(200);
    }
}