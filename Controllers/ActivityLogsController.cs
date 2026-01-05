using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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
                else if (act != "all")
                    query = query.Where(a => a.ActionType.ToLower() == act);
            }

            if (!string.IsNullOrWhiteSpace(filter?.EntityName))
                query = query.Where(a => a.EntityName == filter.EntityName);

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
