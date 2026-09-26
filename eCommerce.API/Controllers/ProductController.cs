using eCommerce.Application.Contracts;
using eCommerce.Application.DTO.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCommerce.Infrastructure.Persistence.Entities;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ProductController(IProductService productService, AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await productService.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await productService.GetProductByIdAsync(id);
            if (product is null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateProductDto request)
        {
            var createdId = await productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdId }, null);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, UpdateProductDto request)
        {
            if (id != request.Id) return BadRequest();

            var updated = await productService.UpdateProductAsync(request);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await productService.DeleteProductAsync(id);

            if (!result.Exists) return NotFound();
            
            if (result.HasOrders) return Conflict(new { message = "Product is referenced by orders and cannot be deleted." });
            
            if (result.Success) return NoContent();
            
            return BadRequest();
        }

        // LINQ demo endpoints
        [HttpGet("linq/top-prices")]
        public async Task<IActionResult> TopPrices(int take = 5)
        {
            var items = await dbContext.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Price)
                .Take(take)
                .Select(p => new { p.Id, p.Name, p.Price, Category = p.Category != null ? p.Category.Name : null })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("linq/stock-summary")]
        public async Task<IActionResult> StockSummary()
        {
            var summary = await dbContext.Products
                .GroupBy(p => new { p.CategoryId, p.Category!.Name })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    Count = g.Count(),
                    TotalStock = g.Sum(x => x.StockQuantity),
                    AvgPrice = g.Average(x => x.Price),
                    Products = g.OrderByDescending(x => x.Price).Select(x => new { x.Id, x.Name, x.Price })
                })
                .ToListAsync();

            return Ok(summary);
        }

        [HttpGet("linq/search")]
        public async Task<IActionResult> Search(string term = "", int page = 1, int pageSize = 10)
        {
            var query = dbContext.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(t) || p.Description.ToLower().Contains(t));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new { p.Id, p.Name, p.Price, p.StockQuantity })
                .ToListAsync();

            return Ok(new { Total = total, Page = page, PageSize = pageSize, Items = items });
        }

        [HttpGet("linq/related")]
        public async Task<IActionResult> Related(int productId)
        {
            var product = await dbContext.Products.FindAsync(productId);
            if (product is null) return NotFound();

            var related = await dbContext.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
                .OrderByDescending(p => p.Price)
                .Select(p => new { p.Id, p.Name, p.Price })
                .ToListAsync();

            return Ok(related);
        }

        [HttpGet("linq/deferred")]
        public async Task<IActionResult> DeferredExecutionDemo()
        {
            // The query is composed but not executed until ToListAsync is called.
            var query = dbContext.Products.Where(p => p.IsActive);

            // mutate query variable
            query = query.Where(p => p.StockQuantity > 0);

            var list = await query.Select(p => new { p.Id, p.Name, p.StockQuantity }).ToListAsync();
            return Ok(list);
        }
    }
}

