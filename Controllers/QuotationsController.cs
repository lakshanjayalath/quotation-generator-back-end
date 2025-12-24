using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs.Quotation;
using quotation_generator_back_end.Models;
using quotation_generator_back_end.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace quotation_generator_back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuotationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public QuotationsController(ApplicationDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }

        /// <summary>
        /// Get all quotations with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuotationListResponseDto>>> GetQuotations(
            [FromQuery] string? status = null,
            [FromQuery] string? search = null)
        {
            try
            {
                // Extract logged-in user's email
                var userEmail = GetLoggedInUserEmail();
                var userRole = GetLoggedInUserRole();
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    return Unauthorized(new { message = "Invalid or missing user email claim" });
                }

                var query = _context.Quotations.AsQueryable();

                // Admin can see all quotations; others see only their own
                if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(q => q.CreatedByEmail == userEmail);
                }

                // Filter by status
                if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
                {
                    query = query.Where(q => q.Status.ToLower() == status.ToLower());
                }

                // Search by client name or quote number
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(q =>
                        (q.ClientName != null && q.ClientName.ToLower().Contains(search.ToLower())) ||
                        q.QuoteNumber.ToLower().Contains(search.ToLower()));
                }

                var quotations = await query
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuotationListResponseDto
                {
                    Id = q.Id,
                    Status = q.Status,
                    QuoteNumber = q.QuoteNumber,
                    ClientName = q.ClientName,
                    Total = q.Total,
                    NetAmount = q.NetAmount,
                    QuoteDate = q.QuoteDate,
                    ValidUntil = q.ValidUntil
                })
                .ToListAsync();

                return Ok(quotations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching quotations", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a single quotation by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<QuotationResponseDto>> GetQuotation(int id)
        {
            // Enforce ownership by email
            var userEmail = GetLoggedInUserEmail();
            var userRole = GetLoggedInUserRole();
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Unauthorized(new { message = "Invalid or missing user email claim" });
            }

            var query = _context.Quotations.AsQueryable();

            // Admin can view all quotations; others can only view their own
            if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(q => q.Id == id);
            }
            else
            {
                query = query.Where(q => q.Id == id && q.CreatedByEmail == userEmail);
            }

            var quotation = await query
                .Include(q => q.Items)
                .FirstOrDefaultAsync();

            if (quotation == null)
            {
                return NotFound(new { message = "Quotation not found" });
            }

            var response = MapToResponseDto(quotation);
            return Ok(response);
        }

        /// <summary>
        /// Create a new quotation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<QuotationResponseDto>> CreateQuotation([FromBody] CreateQuotationDto dto)
        {
            var userEmail = GetLoggedInUserEmail();
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Unauthorized(new { message = "Invalid or missing user email claim" });
            }
            // Calculate totals
            decimal subtotal = 0;
            var items = new List<QuotationItem>();

            foreach (var itemDto in dto.Items)
            {
                var lineTotal = itemDto.UnitCost * itemDto.Quantity;
                subtotal += lineTotal;

                items.Add(new QuotationItem
                {
                    ItemName = itemDto.ItemName,
                    Description = itemDto.Description,
                    UnitCost = itemDto.UnitCost,
                    Quantity = itemDto.Quantity,
                    LineTotal = lineTotal
                });
            }

            // Calculate discount
            decimal discountAmount = 0;
            if (dto.DiscountType?.ToLower() == "percentage")
            {
                discountAmount = subtotal * (dto.Discount / 100);
            }
            else
            {
                discountAmount = dto.Discount;
            }

            var total = subtotal - discountAmount;

            // Get client name if ClientId is provided
            string? clientName = dto.ClientName;
            if (dto.ClientId.HasValue && string.IsNullOrEmpty(clientName))
            {
                var client = await _context.Clients.FindAsync(dto.ClientId.Value);
                clientName = client?.ClientName;
            }

            var quotation = new Quotation
            {
                QuoteNumber = dto.QuoteNumber,
                PoNumber = dto.PoNumber,
                ClientId = dto.ClientId,
                ClientName = clientName,
                QuoteDate = dto.QuoteDate,
                ValidUntil = dto.ValidUntil,
                PartialDeposit = dto.PartialDeposit,
                DiscountType = dto.DiscountType ?? "amount",
                Discount = dto.Discount,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                Total = total,
                NetAmount = total, // Can be adjusted for taxes
                Status = "Draft",
                Project = dto.Project,
                AssignedUser = dto.AssignedUser,
                ExchangeRate = dto.ExchangeRate,
                Vendor = dto.Vendor,
                Design = dto.Design,
                InclusiveTaxes = dto.InclusiveTaxes,
                Items = items,
                CreatedByEmail = userEmail,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync("Quotation", quotation.Id, "Create", $"Created quotation: {quotation.QuoteNumber}");

            var response = MapToResponseDto(quotation);
            return CreatedAtAction(nameof(GetQuotation), new { id = quotation.Id }, response);
        }

        /// <summary>
        /// Update an existing quotation
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<QuotationResponseDto>> UpdateQuotation(int id, [FromBody] UpdateQuotationDto dto)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
            {
                return NotFound(new { message = "Quotation not found" });
            }

            // Update basic fields
            if (!string.IsNullOrEmpty(dto.QuoteNumber))
                quotation.QuoteNumber = dto.QuoteNumber;
            if (dto.PoNumber != null)
                quotation.PoNumber = dto.PoNumber;
            if (dto.ClientId.HasValue)
                quotation.ClientId = dto.ClientId;
            if (dto.ClientName != null)
                quotation.ClientName = dto.ClientName;
            if (dto.QuoteDate.HasValue)
                quotation.QuoteDate = dto.QuoteDate.Value;
            if (dto.ValidUntil.HasValue)
                quotation.ValidUntil = dto.ValidUntil;
            if (dto.PartialDeposit.HasValue)
                quotation.PartialDeposit = dto.PartialDeposit.Value;
            if (!string.IsNullOrEmpty(dto.DiscountType))
                quotation.DiscountType = dto.DiscountType;
            if (dto.Discount.HasValue)
                quotation.Discount = dto.Discount.Value;
            if (!string.IsNullOrEmpty(dto.Status))
                quotation.Status = dto.Status;

            // Update settings
            if (dto.Project != null)
                quotation.Project = dto.Project;
            if (dto.AssignedUser != null)
                quotation.AssignedUser = dto.AssignedUser;
            if (dto.ExchangeRate.HasValue)
                quotation.ExchangeRate = dto.ExchangeRate;
            if (dto.Vendor != null)
                quotation.Vendor = dto.Vendor;
            if (dto.Design != null)
                quotation.Design = dto.Design;
            if (dto.InclusiveTaxes.HasValue)
                quotation.InclusiveTaxes = dto.InclusiveTaxes.Value;

            // Update items if provided
            if (dto.Items != null)
            {
                // Remove old items
                _context.QuotationItems.RemoveRange(quotation.Items);

                // Add new items
                decimal subtotal = 0;
                var newItems = new List<QuotationItem>();

                foreach (var itemDto in dto.Items)
                {
                    var lineTotal = itemDto.UnitCost * itemDto.Quantity;
                    subtotal += lineTotal;

                    newItems.Add(new QuotationItem
                    {
                        QuotationId = id,
                        ItemName = itemDto.ItemName,
                        Description = itemDto.Description,
                        UnitCost = itemDto.UnitCost,
                        Quantity = itemDto.Quantity,
                        LineTotal = lineTotal
                    });
                }

                quotation.Items = newItems;
                quotation.Subtotal = subtotal;

                // Recalculate discount
                decimal discountAmount = 0;
                if (quotation.DiscountType?.ToLower() == "percentage")
                {
                    discountAmount = subtotal * (quotation.Discount / 100);
                }
                else
                {
                    discountAmount = quotation.Discount;
                }

                quotation.DiscountAmount = discountAmount;
                quotation.Total = subtotal - discountAmount;
                quotation.NetAmount = quotation.Total;
            }

            quotation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync("Quotation", id, "Update", $"Updated quotation: {quotation.QuoteNumber}");

            var response = MapToResponseDto(quotation);
            return Ok(response);
        }

        /// <summary>
        /// Update quotation status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<QuotationResponseDto>> UpdateQuotationStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
            {
                return NotFound(new { message = "Quotation not found" });
            }

            quotation.Status = dto.Status;
            quotation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync("Quotation", id, "Update", $"Updated quotation status to: {dto.Status}");

            var response = MapToResponseDto(quotation);
            return Ok(response);
        }

        /// <summary>
        /// Delete a quotation
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuotation(int id)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null)
            {
                return NotFound(new { message = "Quotation not found" });
            }

            var quoteNumber = quotation.QuoteNumber;
            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync("Quotation", id, "Delete", $"Deleted quotation: {quoteNumber}");

            return Ok(new { message = "Quotation deleted successfully" });
        }

        /// <summary>
        /// Get next quote number
        /// </summary>
        [HttpGet("next-number")]
        public async Task<ActionResult<object>> GetNextQuoteNumber()
        {
            var lastQuotation = await _context.Quotations
                .OrderByDescending(q => q.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastQuotation != null)
            {
                // Try to parse the last quote number
                if (int.TryParse(lastQuotation.QuoteNumber, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                else
                {
                    nextNumber = lastQuotation.Id + 1;
                }
            }

            return Ok(new { nextNumber = nextNumber.ToString("D4") });
        }

        private static QuotationResponseDto MapToResponseDto(Quotation quotation)
        {
            return new QuotationResponseDto
            {
                Id = quotation.Id,
                QuoteNumber = quotation.QuoteNumber,
                PoNumber = quotation.PoNumber,
                ClientId = quotation.ClientId,
                ClientName = quotation.ClientName,
                QuoteDate = quotation.QuoteDate,
                ValidUntil = quotation.ValidUntil,
                PartialDeposit = quotation.PartialDeposit,
                DiscountType = quotation.DiscountType,
                Discount = quotation.Discount,
                Subtotal = quotation.Subtotal,
                DiscountAmount = quotation.DiscountAmount,
                Total = quotation.Total,
                NetAmount = quotation.NetAmount,
                Status = quotation.Status,
                Project = quotation.Project,
                AssignedUser = quotation.AssignedUser,
                ExchangeRate = quotation.ExchangeRate,
                Vendor = quotation.Vendor,
                Design = quotation.Design,
                InclusiveTaxes = quotation.InclusiveTaxes,
                Items = quotation.Items.Select(i => new QuotationItemResponseDto
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    Description = i.Description,
                    UnitCost = i.UnitCost,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList(),
                CreatedAt = quotation.CreatedAt,
                UpdatedAt = quotation.UpdatedAt
            };
        }

        private string? GetLoggedInUserEmail()
        {
            // Prefer ClaimTypes.Email, fallback to "email"
            return User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;
        }

        private string? GetLoggedInUserRole()
        {
            // Prefer ClaimTypes.Role, fallback to "role"
            return User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value;
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
