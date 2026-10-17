using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using eCommerce.Domain.Contracts;
using eCommerce.Infrastructure.Persistence.Entities;

namespace eCommerce.Infrastructure.Background
{
    public class OrderProcessingService : BackgroundService
    {
        private readonly IBackgroundJobQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(IBackgroundJobQueue queue, IServiceProvider serviceProvider, ILogger<OrderProcessingService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderProcessingService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var orderId = await _queue.DequeueOrderAsync(stoppingToken);

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var order = await db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId, stoppingToken);
                    if (order == null)
                    {
                        _logger.LogWarning("Order {OrderId} not found.", orderId);
                        continue;
                    }

                    // simulate processing
                    order.Status = "Processed";
                    await db.SaveChangesAsync(stoppingToken);

                    // simulate sending email / invoice by writing file
                    var outboxDir = Path.Combine(AppContext.BaseDirectory, "outbox");
                    Directory.CreateDirectory(outboxDir);
                    var filePath = Path.Combine(outboxDir, $"order-{orderId}.txt");
                    var content = $"Order {order.OrderNumber} processed. Total: {order.TotalAmount}\nItems:\n";
                    foreach (var oi in order.OrderItems)
                    {
                        content += $"- {oi.ProductName} x{oi.Quantity} @ {oi.UnitPrice} = {oi.TotalPrice}\n";
                    }
                    await File.WriteAllTextAsync(filePath, content, stoppingToken);

                    _logger.LogInformation("Order {OrderId} processed and invoice written to {Path}", orderId, filePath);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order");
                    // small delay to prevent tight loop on persistent errors
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            _logger.LogInformation("OrderProcessingService stopping.");
        }
    }
}
