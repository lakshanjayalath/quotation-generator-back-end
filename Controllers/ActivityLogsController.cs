using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs.ActivityLog;
using quotation_generator_back_end.Models;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActivityLogsController> _logger;

        public ActivityLogsController(ApplicationDbContext context, ILogger<ActivityLogsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("my-recent")]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> GetMyRecentActivities([FromQuery] int limit = 5)
        {
            var userId = GetLoggedInUserId();
            var email = GetLoggedInUserEmail()?.Trim().ToLowerInvariant();

            if (!userId.HasValue && string.IsNullOrWhiteSpace(email))
                return Unauthorized(new { message = "Invalid or missing user identity claims" });

            var query = _context.ActivityLogs.AsQueryable();

            // Match by UserId OR by email in PerformedBy, PerformedByEmail
            if (userId.HasValue && !string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(a => 
                    a.UserId == userId || 
                    (a.PerformedByEmail != null && a.PerformedByEmail.ToLower() == email) ||
                    (a.PerformedBy != null && a.PerformedBy.ToLower() == email));
            }
            else if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId);
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(a => 
                    (a.PerformedByEmail != null && a.PerformedByEmail.ToLower() == email) ||
                    (a.PerformedBy != null && a.PerformedBy.ToLower() == email));
            }

            var activities = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .Select(a => new ActivityLogResponseDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    RecordId = a.RecordId,
                    ActionType = a.ActionType,
                    Description = a.Description,
                    PerformedBy = a.PerformedBy,
                    Timestamp = a.Timestamp
                }).ToListAsync();

            return Ok(activities);
        }

        [HttpPost("filter")]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> FilterActivityLogs([FromBody] ActivityLogFilterDto filter)
        {
            try
            {
                var query = _context.ActivityLogs.Include(a => a.User).AsQueryable();
                var isAdmin = string.Equals(GetLoggedInUserRole(), "Admin", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrWhiteSpace(filter?.StartDate) && DateTime.TryParse(filter.StartDate, out var startDate))
                    query = query.Where(a => a.Timestamp >= startDate);

                if (!string.IsNullOrWhiteSpace(filter?.EndDate) && DateTime.TryParse(filter.EndDate, out var endDate))
                    query = query.Where(a => a.Timestamp <= endDate.AddDays(1).AddSeconds(-1));

                if (!string.IsNullOrWhiteSpace(filter?.ActionType))
                {
                    var act = filter.ActionType.Trim().ToLowerInvariant();
                    if (act == "create" || act == "created")
                        query = query.Where(a => a.ActionType.ToLower() == "create");
                    else if (act == "update" || act == "updated")
                        query = query.Where(a => a.ActionType.ToLower() == "update");
                    else if (act == "delete" || act == "deleted")
                        query = query.Where(a => a.ActionType.ToLower() == "delete");
                    else if (act == "login" || act == "logins")
                        query = query.Where(a => a.ActionType.ToLower() == "login");
                    else if (act != "all")
                        query = query.Where(a => a.ActionType.ToLower() == act);
                }

                if (!string.IsNullOrWhiteSpace(filter?.EntityName))
                    query = query.Where(a => a.EntityName == filter.EntityName);

                // For reports we must show all activity (including admin actions) so align with dashboard recent activity
                // by NOT filtering out admin entries here.

                var activities = await query.OrderByDescending(a => a.Timestamp).ToListAsync();

                // Enrich descriptions with Client and Quotation details
                var clientIds = activities?.Where(l => string.Equals(l.EntityName, "Client", StringComparison.OrdinalIgnoreCase)).Select(l => l.RecordId).Distinct().ToList() ?? new List<int>();
                var quoteIds = activities?.Where(l => string.Equals(l.EntityName, "Quotation", StringComparison.OrdinalIgnoreCase) || string.Equals(l.EntityName, "Quote", StringComparison.OrdinalIgnoreCase)).Select(l => l.RecordId).Distinct().ToList() ?? new List<int>();
                
                var clientsById = new Dictionary<int, Client>();
                var quotesById = new Dictionary<int, Quotation>();

                if (clientIds?.Count > 0)
                {
                    clientsById = await _context.Clients.Where(c => clientIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
                }
                
                if (quoteIds?.Count > 0)
                {
                    quotesById = await _context.Quotations.Where(q => quoteIds.Contains(q.Id)).ToDictionaryAsync(q => q.Id);
                }

                var result = new List<ActivityLogResponseDto>();
                if (activities != null)
                {
                    foreach (var a in activities)
                    {
                        var desc = a.Description ?? string.Empty;

                        // Enrich with Client details
                        if (string.Equals(a.EntityName, "Client", StringComparison.OrdinalIgnoreCase) && clientsById.TryGetValue(a.RecordId, out var client))
                        {
                            var name = client.ClientName ?? "Client";
                            var email = client.ClientEmail ?? string.Empty;
                            var phone = client.ClientContactNumber ?? client.Phone ?? string.Empty;
                            desc = string.IsNullOrWhiteSpace(desc)
                                ? $"{name} (Email: {email}, Phone: {phone})"
                                : $"{desc} | {name} (Email: {email}, Phone: {phone})";
                        }
                        // Enrich with Quotation details
                        else if ((string.Equals(a.EntityName, "Quotation", StringComparison.OrdinalIgnoreCase) || string.Equals(a.EntityName, "Quote", StringComparison.OrdinalIgnoreCase)) && quotesById.TryGetValue(a.RecordId, out var quote))
                        {
                            var qClient = quote.ClientName ?? string.Empty;
                            var total = quote.Total;
                            var status = quote.Status ?? string.Empty;
                            var date = quote.QuoteDate.ToString("yyyy-MM-dd");
                            desc = string.IsNullOrWhiteSpace(desc)
                                ? $"Client: {qClient}, Total: {total}, Status: {status}, Date: {date}"
                                : $"{desc} | Client: {qClient}, Total: {total}, Status: {status}, Date: {date}";
                        }

                        result.Add(new ActivityLogResponseDto
                        {
                            Id = a.Id,
                            EntityName = a.EntityName,
                            RecordId = a.RecordId,
                            ActionType = a.ActionType,
                            Description = desc,
                            PerformedBy = a.PerformedBy,
                            Timestamp = a.Timestamp
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterActivityLogs");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> GetAllActivityLogs()
        {
            var isAdmin = string.Equals(GetLoggedInUserRole(), "Admin", StringComparison.OrdinalIgnoreCase);
            var query = _context.ActivityLogs.Include(a => a.User).AsQueryable();

            if (!isAdmin)
                query = query.Where(a => a.User == null ? !EF.Functions.Like(a.PerformedBy ?? "", "%admin%") : a.User.Role != "Admin");

            var result = await query.OrderByDescending(a => a.Timestamp)
                .Select(a => new ActivityLogResponseDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    RecordId = a.RecordId,
                    ActionType = a.ActionType,
                    Description = a.Description,
                    PerformedBy = a.PerformedBy,
                    Timestamp = a.Timestamp
                }).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{entityName}/{recordId}")]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> GetActivityLogsForEntity(string entityName, int recordId)
        {
            var isAdmin = string.Equals(GetLoggedInUserRole(), "Admin", StringComparison.OrdinalIgnoreCase);

            var query = _context.ActivityLogs.Include(a => a.User)
                        .Where(a => a.EntityName == entityName && a.RecordId == recordId);

            if (!isAdmin)
                query = query.Where(a => a.User == null ? !EF.Functions.Like(a.PerformedBy ?? "", "%admin%") : a.User.Role != "Admin");

            var result = await query.OrderByDescending(a => a.Timestamp)
                .Select(a => new ActivityLogResponseDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    RecordId = a.RecordId,
                    ActionType = a.ActionType,
                    Description = a.Description,
                    PerformedBy = a.PerformedBy,
                    Timestamp = a.Timestamp
                }).ToListAsync();

            return Ok(result);
        }

        // Helper Methods
        private string? GetLoggedInUserRole() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

        private int? GetLoggedInUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return int.TryParse(id, out var parsed) ? parsed : (int?)null;
        }

        private string? GetLoggedInUserEmail() =>
            User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value ?? User.Identity?.Name;
    }
}
