using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities.Products;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .Include(c => c.Children)
                .ToListAsync();
            return Ok(categories);
        }

        // GET: api/Category/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            category.CategoryId = Guid.NewGuid();
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
        }

        // PUT: api/Category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Category category)
        {
            if (id != category.CategoryId)
                return BadRequest("Id mismatch");

            var exist = await _context.Categories.FindAsync(id);
            if (exist == null)
                return NotFound();

            exist.Name = category.Name;
            exist.ParentId = category.ParentId;
            await _context.SaveChangesAsync();
            return Ok(exist);
        }

        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}