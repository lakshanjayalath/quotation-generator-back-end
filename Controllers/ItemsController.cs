using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs;
using quotation_generator_back_end.Models;
using quotation_generator_back_end.Services;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public ItemsController(ApplicationDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemResponseDto>>> GetItems(
            [FromQuery] string? filter,
            [FromQuery] bool? isActive)
        {
            var query = _context.Items.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(i => 
                    i.Title.Contains(filter) || 
                    i.Description.Contains(filter));
            }

            if (isActive.HasValue)
            {
                query = query.Where(i => i.IsActive == isActive.Value);
            }

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new ItemResponseDto
                {
                    Id = i.Id,
                    Item = i.Title,
                    Description = i.Description,
                    Price = $"{i.Price:N0} LKR",
                    Qty = i.Quantity,
                    ImageUrl = i.ImageUrl,
                    IsActive = i.IsActive
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found" });
            }

            return Ok(item);
        }

        // POST: api/Items
        [HttpPost]
        public async Task<ActionResult<Item>> CreateItem(CreateItemDto createItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed for item creation",
                    errors = GetModelErrors()
                });
            }

            // Validate data
            if (string.IsNullOrWhiteSpace(createItemDto.Title))
            {
                return BadRequest(new { message = "Title is required" });
            }

            if (createItemDto.Price <= 0)
            {
                return BadRequest(new { message = "Price must be greater than 0" });
            }

            if (createItemDto.Quantity <= 0)
            {
                return BadRequest(new { message = "Quantity must be greater than 0" });
            }

            var item = new Item
            {
                Title = createItemDto.Title,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                Quantity = createItemDto.Quantity,
                ImageUrl = createItemDto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync("Item", item.Id, "Create", $"Created item: {item.Title}");

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }

        // PUT: api/Items/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, UpdateItemDto updateItemDto)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found" });
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateItemDto.Title))
                item.Title = updateItemDto.Title;

            if (!string.IsNullOrEmpty(updateItemDto.Description))
                item.Description = updateItemDto.Description;

            if (updateItemDto.Price.HasValue && updateItemDto.Price.Value > 0)
                item.Price = updateItemDto.Price.Value;

            if (updateItemDto.Quantity.HasValue && updateItemDto.Quantity.Value > 0)
                item.Quantity = updateItemDto.Quantity.Value;

            if (updateItemDto.ImageUrl != null)
                item.ImageUrl = updateItemDto.ImageUrl;

            if (updateItemDto.IsActive.HasValue)
                item.IsActive = updateItemDto.IsActive.Value;

            item.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("Item", id, "Update", $"Updated item: {item.Title}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            
            if (item == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found" });
            }

            var itemTitle = item.Title;
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync("Item", id, "Delete", $"Deleted item: {itemTitle}");

            return NoContent();
        }

        // DELETE: api/Items/bulk
        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteItems([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(new { message = "No item IDs provided" });
            }

            var items = await _context.Items
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (items.Count == 0)
            {
                return NotFound(new { message = "No items found with the provided IDs" });
            }

            _context.Items.RemoveRange(items);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                await _activityLogger.LogAsync("Item", item.Id, "Delete", $"Bulk deleted item: {item.Title}");
            }

            return Ok(new { message = $"{items.Count} item(s) deleted successfully" });
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }

        private string[] GetModelErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
                .ToArray();
        }
    }
}
