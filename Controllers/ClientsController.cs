using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs;
using quotation_generator_back_end.Models;
using quotation_generator_back_end.Services;
using System.Security.Claims;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public ClientsController(ApplicationDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }

        // Helper method to get current user's email from JWT claims
        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value 
                ?? User.FindFirst("email")?.Value 
                ?? User.Identity?.Name 
                ?? "System";
        }

        // GET: api/Clients/next-id
        [HttpGet("next-id")]
        public async Task<ActionResult<NextClientIdDto>> GetNextClientId()
        {
            try
            {
                var lastClient = await _context.Clients
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                var nextId = (lastClient?.Id ?? 0) + 1;
                var formattedId = $"CLT-{nextId:D6}";

                return Ok(new NextClientIdDto
                {
                    NextId = nextId,
                    FormattedClientId = formattedId,
                    Message = "Next Client ID available"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get next client ID", error = ex.Message });
            }
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientResponseDto>>> GetClients(
            [FromQuery] string? filter,
            [FromQuery] bool? isActive)
        {
            try
            {
                var query = _context.Clients.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var term = filter.Trim().ToLowerInvariant();
                    query = query.Where(c =>
                        (!string.IsNullOrEmpty(c.ClientName) && c.ClientName.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(c.ClientEmail) && c.ClientEmail.ToLower().Contains(term)));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                var clients = await query
                    .OrderByDescending(c => c.CreatedDate)
                    .Select(c => new ClientResponseDto
                    {
                        ClientId = c.Id,
                        Name = c.ClientName,
                        CompanyName = c.Name,
                        Email = c.ClientEmail,
                        ContactNumber = c.ClientContactNumber,
                        CreatedDate = c.CreatedDate.ToString("yyyy-MM-dd"),
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch clients", error = ex.Message });
            }
        }

        // POST: api/Clients
        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient(CreateClientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.ClientName))
                return BadRequest(new { message = "Client name is required" });

            if (string.IsNullOrWhiteSpace(dto.ClientEmail))
                return BadRequest(new { message = "Client email is required" });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var currentUserEmail = GetCurrentUserEmail();
                var assignedUser = !string.IsNullOrWhiteSpace(dto.AssignedUser) 
                    ? dto.AssignedUser 
                    : currentUserEmail;

                var client = new Client
                {
                    ClientName = dto.ClientName,
                    ClientIdNumber = dto.ClientIdNumber,
                    ClientContactNumber = dto.ClientContactNumber,
                    ClientAddress = dto.ClientAddress,
                    ClientEmail = dto.ClientEmail,
                    Name = dto.Name,
                    Number = dto.Number,
                    Group = dto.Group,
                    AssignedUser = assignedUser,
                    IdNumber = dto.IdNumber,
                    VatNumber = dto.VatNumber,
                    Website = dto.Website,
                    Phone = dto.Phone,
                    RoutingId = dto.RoutingId,
                    ValidVat = dto.ValidVat,
                    TaxExempt = dto.TaxExempt,
                    Classification = dto.Classification,
                    BillingStreet = dto.BillingStreet,
                    BillingSuite = dto.BillingSuite,
                    BillingCity = dto.BillingCity,
                    BillingState = dto.BillingState,
                    BillingPostalCode = dto.BillingPostalCode,
                    BillingCountry = dto.BillingCountry,
                    ShippingStreet = dto.ShippingStreet,
                    ShippingSuite = dto.ShippingSuite,
                    ShippingCity = dto.ShippingCity,
                    ShippingState = dto.ShippingState,
                    ShippingPostalCode = dto.ShippingPostalCode,
                    ShippingCountry = dto.ShippingCountry,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    Contacts = new List<ClientContact>()
                };

                if (dto.Contacts != null)
                {
                    foreach (var c in dto.Contacts)
                    {
                        client.Contacts.Add(new ClientContact
                        {
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            Email = c.Email,
                            Phone = c.Phone,
                            AddToInvoices = c.AddToInvoices
                        });
                    }
                }

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                client.ClientIdFormatted = $"CLT-{client.Id:D6}";
                await _context.SaveChangesAsync();

                // ✅ Log activity
                await _activityLogger.LogAsync(
                    "Client",
                    client.Id,
                    "Create",
                    $"Created client: {client.ClientName} ({client.ClientIdFormatted})",
                    currentUserEmail
                );

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound();

            return Ok(client);
        }

        // PUT: api/Clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, UpdateClientDto dto)
        {
            var client = await _context.Clients
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound();

            client.ClientName = dto.ClientName ?? client.ClientName;
            client.ClientEmail = dto.ClientEmail ?? client.ClientEmail;
            client.IsActive = dto.IsActive ?? client.IsActive;
            client.UpdatedAt = DateTime.UtcNow;

            if (dto.Contacts != null)
            {
                _context.ClientContacts.RemoveRange(client.Contacts);
                client.Contacts.Clear();

                foreach (var c in dto.Contacts)
                {
                    client.Contacts.Add(new ClientContact
                    {
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Email = c.Email,
                        Phone = c.Phone,
                        AddToInvoices = c.AddToInvoices
                    });
                }
            }

            await _context.SaveChangesAsync();

            // ✅ Log activity
            var currentUserEmail = GetCurrentUserEmail();
            await _activityLogger.LogAsync(
                "Client",
                id,
                "Update",
                $"Updated client: {client.ClientName}",
                currentUserEmail
            );

            return NoContent();
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            // ✅ Log activity
            var currentUserEmail = GetCurrentUserEmail();
            await _activityLogger.LogAsync(
                "Client",
                id,
                "Delete",
                $"Deleted client: {client.ClientName}",
                currentUserEmail
            );

            return NoContent();
        }

        // DELETE: api/Clients/bulk
        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDeleteClients([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest(new { message = "No client IDs provided" });

            var clients = await _context.Clients
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (!clients.Any())
                return NotFound(new { message = "No matching clients found" });

            _context.Clients.RemoveRange(clients);
            await _context.SaveChangesAsync();

            // ✅ Log each deletion
            var currentUserEmail = GetCurrentUserEmail();
            foreach (var c in clients)
            {
                await _activityLogger.LogAsync(
                    "Client",
                    c.Id,
                    "Delete",
                    $"Deleted client: {c.ClientName}",
                    currentUserEmail
                );
            }

            return NoContent();
        }
    }
}
