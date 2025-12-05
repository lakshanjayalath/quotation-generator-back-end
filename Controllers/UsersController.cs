using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs;
using quotation_generator_back_end.Helpers;
using quotation_generator_back_end.Models;
using quotation_generator_back_end.Services;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public UsersController(ApplicationDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserSettingsDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            var dto = new UserSettingsDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Language = user.Language,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IdNumber = user.IdNumber,
                TwoFactorAuth = user.TwoFactorAuth,
                LoginNotification = user.LoginNotification,
                TaskAssignNotification = user.TaskAssignNotification,
                DisableRecurringPaymentNotification = user.DisableRecurringPaymentNotification,
                AllEvents = user.AllEvents,
                InvoiceCreated = user.InvoiceCreated,
                InvoiceSent = user.InvoiceSent,
                QuoteCreated = user.QuoteCreated,
                QuoteSent = user.QuoteSent,
                QuoteView = user.QuoteView,
                PaymentDetails = user.PaymentDetails
            };

            return Ok(dto);
        }

        // PUT: api/Users/5/settings
        [HttpPut("{id}/settings")]
        public async Task<IActionResult> UpdateUserSettings(int id, UserSettingsDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            // Update basic fields
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Language = dto.Language;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address = dto.Address;
            user.IdNumber = dto.IdNumber;

            // Password change - optional
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                if (string.IsNullOrEmpty(dto.CurrentPassword))
                {
                    return BadRequest(new { message = "Current password is required to change password." });
                }

                if (string.IsNullOrEmpty(user.PasswordHash) || !PasswordHelper.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { message = "Current password is incorrect." });
                }

                user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
            }

            // Notifications & auth
            user.TwoFactorAuth = dto.TwoFactorAuth;
            user.LoginNotification = dto.LoginNotification;
            user.TaskAssignNotification = dto.TaskAssignNotification;
            user.DisableRecurringPaymentNotification = dto.DisableRecurringPaymentNotification;

            user.AllEvents = dto.AllEvents;
            user.InvoiceCreated = dto.InvoiceCreated;
            user.InvoiceSent = dto.InvoiceSent;
            user.QuoteCreated = dto.QuoteCreated;
            user.QuoteSent = dto.QuoteSent;
            user.QuoteView = dto.QuoteView;
            user.PaymentDetails = dto.PaymentDetails;

            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("User", id, "Update", $"Updated user settings: {user.Email}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        private bool UserExists(int id) => _context.Users.Any(u => u.Id == id);
    }
}
