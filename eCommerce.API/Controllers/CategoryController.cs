using eCommerce.Application.Contracts;
using eCommerce.Application.DTO.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCommerce.Infrastructure.Persistence.Entities;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService, AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var categories = await categoryService.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateCategoryDto request)
        {
            var createdId = await categoryService.AddCategoryAsync(request);
            
            return CreatedAtAction(nameof(GetById), new { id = createdId }, null);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await categoryService.DeleteCategoryAsync(id);

            if (!result.Exists)
                return NotFound();

            if (result.HasProducts)
                return Conflict(new { message = "Category has products and cannot be deleted." });

            if (result.Success)
                return NoContent();

            return BadRequest();
        }

        // LINQ demo endpoints for categories
        [HttpGet("linq/categories-with-product-count")]
        public async Task<IActionResult> CategoriesWithCounts()
        {
            var data = await dbContext.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    ProductCount = c.Products.Count,
                    ActiveProducts = c.Products.Count(p => p.IsActive),
                    InactiveProducts = c.Products.Count(p => !p.IsActive)
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("linq/top-categories-by-revenue")]
        public async Task<IActionResult> TopCategoriesByRevenue(int take = 5)
        {
            // Join OrderItems -> Product -> Category and sum totals per category
            var data = await dbContext.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Product != null)
                .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category != null ? oi.Product.Category.Name : null })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Revenue = g.Sum(x => x.TotalPrice),
                    UnitsSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(take)
                .ToListAsync();

            return Ok(data);
        }


    }
}
