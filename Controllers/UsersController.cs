using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs;
using quotation_generator_back_end.DTOs.UserProfile;
using quotation_generator_back_end.Helpers;
using quotation_generator_back_end.Models;
using quotation_generator_back_end.Services;
using System.Security.Claims;

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

        #region Profile Endpoints

        /// <summary>
        /// Get current user's profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Get quotation summary
            var quotations = await _context.Quotations
                .Where(q => q.AssignedUser == user.Email || q.AssignedUser == $"{user.FirstName} {user.LastName}")
                .ToListAsync();

            var quotationSummary = new QuotationSummaryDto
            {
                Total = quotations.Count,
                Pending = quotations.Count(q => q.Status == "Draft" || q.Status == "Sent"),
                Approved = quotations.Count(q => q.Status == "Accepted"),
                Rejected = quotations.Count(q => q.Status == "Declined" || q.Status == "Expired"),
                TotalValue = quotations.Sum(q => q.Total)
            };

            var recentQuotations = quotations
                .OrderByDescending(q => q.CreatedAt)
                .Take(10)
                .Select(q => new QuotationHistoryDto
                {
                    Id = q.Id,
                    QuoteNo = q.QuoteNumber,
                    Date = q.QuoteDate,
                    Status = q.Status,
                    Amount = q.Total
                })
                .ToList();

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role,
                Street = user.Street,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                Country = user.Country,
                Language = user.Language ?? "English",
                PreferredContactMethod = user.PreferredContactMethod ?? "Email",
                Notifications = user.NotificationsEnabled,
                Notes = user.Notes,
                QuotationSummary = quotationSummary,
                Quotations = recentQuotations,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(profileDto);
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if email is being changed and if it's already taken
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email.ToLower() != user.Email?.ToLower())
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == dto.Email.ToLower() && u.Id != userId);
                if (emailExists)
                {
                    return BadRequest(new { message = "Email is already in use by another account" });
                }
                user.Email = dto.Email.ToLower();
            }

            // Update basic info
            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.Phone != null) user.PhoneNumber = dto.Phone;

            // Update address
            if (dto.Street != null) user.Street = dto.Street;
            if (dto.City != null) user.City = dto.City;
            if (dto.State != null) user.State = dto.State;
            if (dto.PostalCode != null) user.PostalCode = dto.PostalCode;
            if (dto.Country != null) user.Country = dto.Country;

            // Update preferences
            if (dto.Language != null) user.Language = dto.Language;
            if (dto.PreferredContactMethod != null) user.PreferredContactMethod = dto.PreferredContactMethod;
            user.NotificationsEnabled = dto.Notifications;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _activityLogger.LogAsync("User", user.Id, "Update", $"Updated profile: {user.Email}");

            return Ok(new { message = "Profile updated successfully" });
        }

        /// <summary>
        /// Change current user's password
        /// </summary>
        [HttpPut("profile/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Verify current password
            if (string.IsNullOrEmpty(user.PasswordHash) || 
                !PasswordHelper.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            // Validate new password
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return BadRequest(new { message = "New passwords do not match" });
            }

            // Update password
            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _activityLogger.LogAsync("User", user.Id, "Update", $"Changed password: {user.Email}");

            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Update current user's notes
        /// </summary>
        [HttpPut("profile/notes")]
        [Authorize]
        public async Task<IActionResult> UpdateNotes([FromBody] UpdateNotesDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.Notes = dto.Notes;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _activityLogger.LogAsync("User", user.Id, "Update", $"Updated notes: {user.Email}");

            return Ok(new { message = "Notes saved successfully" });
        }

        /// <summary>
        /// Update profile image URL (from Supabase) - accepts FormData
        /// </summary>
        [HttpPost("profile/image")]
        [Authorize]
        public async Task<ActionResult<ProfileImageResponseDto>> UpdateProfileImage([FromForm] UpdateProfileImageDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ProfileImageResponseDto 
                { 
                    Success = false, 
                    Message = "User not authenticated" 
                });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new ProfileImageResponseDto 
                { 
                    Success = false, 
                    Message = "User not found" 
                });
            }

            if (string.IsNullOrEmpty(dto.ImageUrl))
            {
                return BadRequest(new ProfileImageResponseDto 
                { 
                    Success = false, 
                    Message = "Image URL is required" 
                });
            }

            // Validate that it's a valid URL
            if (!Uri.TryCreate(dto.ImageUrl, UriKind.Absolute, out var uriResult) 
                || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest(new ProfileImageResponseDto 
                { 
                    Success = false, 
                    Message = "Invalid URL format" 
                });
            }

            try
            {
                // Update user profile with Supabase URL
                user.ProfileImageUrl = dto.ImageUrl;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("User", user.Id, "Update", $"Updated profile image: {user.Email}");

                return Ok(new ProfileImageResponseDto 
                { 
                    Success = true, 
                    Message = "Profile image updated successfully",
                    ImageUrl = dto.ImageUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProfileImageResponseDto 
                { 
                    Success = false, 
                    Message = $"Error updating profile image: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Delete profile image
        /// </summary>
        [HttpDelete("profile/image")]
        [Authorize]
        public async Task<IActionResult> DeleteProfileImage()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                user.ProfileImageUrl = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await _activityLogger.LogAsync("User", user.Id, "Update", $"Deleted profile image: {user.Email}");
            }

            return Ok(new { message = "Profile image deleted successfully" });
        }

        /// <summary>
        /// Get user's quotation history
        /// </summary>
        [HttpGet("profile/quotations")]
        [Authorize]
        public async Task<ActionResult<List<QuotationHistoryDto>>> GetQuotationHistory(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var query = _context.Quotations
                .Where(q => q.AssignedUser == user.Email || q.AssignedUser == $"{user.FirstName} {user.LastName}");

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(q => q.Status == status);
            }

            var totalCount = await query.CountAsync();
            var quotations = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuotationHistoryDto
                {
                    Id = q.Id,
                    QuoteNo = q.QuoteNumber,
                    Date = q.QuoteDate,
                    Status = q.Status,
                    Amount = q.Total
                })
                .ToListAsync();

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());
            Response.Headers.Append("X-Total-Pages", ((int)Math.Ceiling((double)totalCount / pageSize)).ToString());

            return Ok(quotations);
        }

        /// <summary>
        /// Helper method to get current user ID from JWT claims
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                ?? User.FindFirst("sub") 
                ?? User.FindFirst("id");
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        #endregion

        #region Existing Endpoints

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

        #endregion
    }
}
