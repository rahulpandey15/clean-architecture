using eCommerce.Application.DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCommerce.Infrastructure.Persistence.Entities;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController(AppDbContext dbContext) : ControllerBase
    {


        [HttpGet]
  
        public async Task<IActionResult> Get()
        {
            var orderResponse
                 = new OrderResponse()
                 {
                     OrderName = "Shoes"
                 };
            return Ok(orderResponse);
        }



        [HttpPost]
        public async Task<IActionResult> Post(CreateOrderDto createOrderDto) {

            return Ok();
        }

        // LINQ demo endpoints for orders
        [HttpGet("linq/recent")]
        public async Task<IActionResult> RecentOrders(int take = 10)
        {
            var orders = await dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedOn)
                .Take(take)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    User = o.User != null ? new { o.User.Id, o.User.FirstName, o.User.LastName, o.User.Email } : null,
                    Items = o.OrderItems.Select(oi => new { oi.ProductId, oi.ProductName, oi.Quantity, oi.UnitPrice, oi.TotalPrice }),
                    Total = o.OrderItems.Sum(oi => oi.TotalPrice),
                    o.Status,
                    o.CreatedOn
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("linq/high-value-customers")]
        public async Task<IActionResult> HighValueCustomers(decimal threshold = 500)
        {
            var data = await dbContext.Orders
                .GroupBy(o => o.UserId)
                .Select(g => new { UserId = g.Key, TotalSpent = g.Sum(o => o.TotalAmount) })
                .Where(x => x.TotalSpent >= threshold)
                .Join(dbContext.Users, g => g.UserId, u => u.Id, (g, u) => new { u.Id, u.FirstName, u.LastName, u.Email, g.TotalSpent })
                .OrderByDescending(x => x.TotalSpent)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("linq/product-sales")]
        public async Task<IActionResult> ProductSales(int take = 50)
        {
            var sales = await dbContext.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(take)
                .ToListAsync();

            return Ok(sales);
        }

        // two action
        // 1. create order (post)
        // 2. GetOrderDetail(GET)
    }

    internal class OrderResponse
    {
        public string OrderName {  get; set; }
    }
}
