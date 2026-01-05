using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.Models;
using System.Threading.Tasks;

namespace quotation_generator_back_end.Services
{
    public interface IActivityLogger
    {
        Task LogAsync(
            string entityName,
            int recordId,
            string actionType,
            string? description = null,
            string? performedBy = null);
    }

    public class ActivityLogger : IActivityLogger
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly TimeSpan SriLankaOffset = TimeSpan.FromHours(5.5);

        public ActivityLogger(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string entityName,
            int recordId,
            string actionType,
            string? description = null,
            string? performedBy = null)
        {
            try
            {
                var timestamp = DateTime.UtcNow.Add(SriLankaOffset);

                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                int? userId = null;
                string performedByEmail = "System";
                string? performedByRole = null;

                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userIdValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                      ?? user.FindFirst("sub")?.Value;

                    if (int.TryParse(userIdValue, out var parsedId))
                        userId = parsedId;

                    performedByEmail = user.FindFirst(ClaimTypes.Email)?.Value
                                       ?? user.FindFirst("email")?.Value
                                       ?? user.Identity?.Name
                                       ?? "System";

                    performedByRole = user.FindFirst(ClaimTypes.Role)?.Value
                                      ?? user.FindFirst("role")?.Value;
                }

                if (string.IsNullOrWhiteSpace(performedByEmail))
                    performedByEmail = "System";

                var performedByValue = string.IsNullOrWhiteSpace(performedBy)
                    ? performedByEmail
                    : performedBy;

                if (string.IsNullOrWhiteSpace(performedByValue))
                    performedByValue = "System";

                var log = new ActivityLog
                {
                    EntityName = entityName,
                    RecordId = recordId,
                    ActionType = actionType,
                    Description = description ?? $"{actionType} {entityName}",
                    PerformedBy = performedByValue,
                    PerformedByEmail = performedByEmail,
                    PerformedByRole = performedByRole,
                    UserId = userId,
                    Timestamp = timestamp
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // ❗ Never break main flow
                Console.WriteLine($"Activity logging failed: {ex}");
            }
        }
    }
}
