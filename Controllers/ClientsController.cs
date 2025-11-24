using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs;
using quotation_generator_back_end.Models;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientResponseDto>>> GetClients(
            [FromQuery] string? filter,
            [FromQuery] bool? isActive)
        {
            var query = _context.Clients.Include(c => c.Contacts).AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(c => 
                    c.ClientName.Contains(filter) || 
                    c.Name.Contains(filter) ||
                    c.ClientEmail.Contains(filter));
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

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }

            return Ok(client);
        }

        // POST: api/Clients
        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient(CreateClientDto createClientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(createClientDto.ClientName))
            {
                return BadRequest(new { message = "Client name is required" });
            }

            if (string.IsNullOrWhiteSpace(createClientDto.ClientEmail))
            {
                return BadRequest(new { message = "Client email is required" });
            }

            var client = new Client
            {
                ClientName = createClientDto.ClientName,
                ClientIdNumber = createClientDto.ClientIdNumber,
                ClientContactNumber = createClientDto.ClientContactNumber,
                ClientAddress = createClientDto.ClientAddress,
                ClientEmail = createClientDto.ClientEmail,
                Name = createClientDto.Name,
                Number = createClientDto.Number,
                Group = createClientDto.Group,
                AssignedUser = createClientDto.AssignedUser,
                IdNumber = createClientDto.IdNumber,
                VatNumber = createClientDto.VatNumber,
                Website = createClientDto.Website,
                Phone = createClientDto.Phone,
                RoutingId = createClientDto.RoutingId,
                ValidVat = createClientDto.ValidVat,
                TaxExempt = createClientDto.TaxExempt,
                Classification = createClientDto.Classification,
                BillingStreet = createClientDto.BillingStreet,
                BillingSuite = createClientDto.BillingSuite,
                BillingCity = createClientDto.BillingCity,
                BillingState = createClientDto.BillingState,
                BillingPostalCode = createClientDto.BillingPostalCode,
                BillingCountry = createClientDto.BillingCountry,
                ShippingStreet = createClientDto.ShippingStreet,
                ShippingSuite = createClientDto.ShippingSuite,
                ShippingCity = createClientDto.ShippingCity,
                ShippingState = createClientDto.ShippingState,
                ShippingPostalCode = createClientDto.ShippingPostalCode,
                ShippingCountry = createClientDto.ShippingCountry,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            // Add contacts
            foreach (var contactDto in createClientDto.Contacts)
            {
                client.Contacts.Add(new ClientContact
                {
                    FirstName = contactDto.FirstName,
                    LastName = contactDto.LastName,
                    Email = contactDto.Email,
                    Phone = contactDto.Phone,
                    AddToInvoices = contactDto.AddToInvoices
                });
            }

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }

        // PUT: api/Clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, UpdateClientDto updateClientDto)
        {
            var client = await _context.Clients
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }

            // Update client fields if provided
            if (!string.IsNullOrEmpty(updateClientDto.ClientName))
                client.ClientName = updateClientDto.ClientName;
            
            if (!string.IsNullOrEmpty(updateClientDto.ClientIdNumber))
                client.ClientIdNumber = updateClientDto.ClientIdNumber;
            
            if (!string.IsNullOrEmpty(updateClientDto.ClientContactNumber))
                client.ClientContactNumber = updateClientDto.ClientContactNumber;
            
            if (!string.IsNullOrEmpty(updateClientDto.ClientAddress))
                client.ClientAddress = updateClientDto.ClientAddress;
            
            if (!string.IsNullOrEmpty(updateClientDto.ClientEmail))
                client.ClientEmail = updateClientDto.ClientEmail;
            
            if (!string.IsNullOrEmpty(updateClientDto.Name))
                client.Name = updateClientDto.Name;
            
            if (!string.IsNullOrEmpty(updateClientDto.Number))
                client.Number = updateClientDto.Number;
            
            if (!string.IsNullOrEmpty(updateClientDto.Group))
                client.Group = updateClientDto.Group;
            
            if (!string.IsNullOrEmpty(updateClientDto.AssignedUser))
                client.AssignedUser = updateClientDto.AssignedUser;
            
            if (!string.IsNullOrEmpty(updateClientDto.IdNumber))
                client.IdNumber = updateClientDto.IdNumber;
            
            if (!string.IsNullOrEmpty(updateClientDto.VatNumber))
                client.VatNumber = updateClientDto.VatNumber;
            
            if (!string.IsNullOrEmpty(updateClientDto.Website))
                client.Website = updateClientDto.Website;
            
            if (!string.IsNullOrEmpty(updateClientDto.Phone))
                client.Phone = updateClientDto.Phone;
            
            if (!string.IsNullOrEmpty(updateClientDto.RoutingId))
                client.RoutingId = updateClientDto.RoutingId;
            
            if (updateClientDto.ValidVat.HasValue)
                client.ValidVat = updateClientDto.ValidVat.Value;
            
            if (updateClientDto.TaxExempt.HasValue)
                client.TaxExempt = updateClientDto.TaxExempt.Value;
            
            if (!string.IsNullOrEmpty(updateClientDto.Classification))
                client.Classification = updateClientDto.Classification;
            
            // Update addresses
            if (!string.IsNullOrEmpty(updateClientDto.BillingStreet))
                client.BillingStreet = updateClientDto.BillingStreet;
            
            if (!string.IsNullOrEmpty(updateClientDto.BillingSuite))
                client.BillingSuite = updateClientDto.BillingSuite;
            
            if (!string.IsNullOrEmpty(updateClientDto.BillingCity))
                client.BillingCity = updateClientDto.BillingCity;
            
            if (!string.IsNullOrEmpty(updateClientDto.BillingState))
                client.BillingState = updateClientDto.BillingState;
            
            if (!string.IsNullOrEmpty(updateClientDto.BillingPostalCode))
                client.BillingPostalCode = updateClientDto.BillingPostalCode;
            
            if (!string.IsNullOrEmpty(updateClientDto.BillingCountry))
                client.BillingCountry = updateClientDto.BillingCountry;
            
            if (!string.IsNullOrEmpty(updateClientDto.ShippingStreet))
                client.ShippingStreet = updateClientDto.ShippingStreet;
            
            if (!string.IsNullOrEmpty(updateClientDto.ShippingSuite))
                client.ShippingSuite = updateClientDto.ShippingSuite;
            
            if (!string.IsNullOrEmpty(updateClientDto.ShippingCity))
                client.ShippingCity = updateClientDto.ShippingCity;
            
            if (!string.IsNullOrEmpty(updateClientDto.ShippingState))
                client.ShippingState = updateClientDto.ShippingState;
            
            if (!string.IsNullOrEmpty(updateClientDto.ShippingPostalCode))
                client.ShippingPostalCode = updateClientDto.ShippingPostalCode;
            
            if (!string.IsNullOrEmpty(updateClientDto.ShippingCountry))
                client.ShippingCountry = updateClientDto.ShippingCountry;
            
            if (updateClientDto.IsActive.HasValue)
                client.IsActive = updateClientDto.IsActive.Value;

            // Update contacts if provided
            if (updateClientDto.Contacts != null)
            {
                // Remove old contacts
                _context.ClientContacts.RemoveRange(client.Contacts);
                
                // Add new contacts
                client.Contacts.Clear();
                foreach (var contactDto in updateClientDto.Contacts)
                {
                    client.Contacts.Add(new ClientContact
                    {
                        FirstName = contactDto.FirstName,
                        LastName = contactDto.LastName,
                        Email = contactDto.Email,
                        Phone = contactDto.Phone,
                        AddToInvoices = contactDto.AddToInvoices
                    });
                }
            }

            client.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
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

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            
            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {id} not found" });
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Clients/bulk
        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteClients([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(new { message = "No client IDs provided" });
            }

            var clients = await _context.Clients
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (clients.Count == 0)
            {
                return NotFound(new { message = "No clients found with the provided IDs" });
            }

            _context.Clients.RemoveRange(clients);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{clients.Count} client(s) deleted successfully" });
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
