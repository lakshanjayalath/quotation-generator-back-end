using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs.ActivityLog;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Filter activity logs by date range and action type
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>List of filtered activity logs</returns>
        [HttpPost("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> FilterActivityLogs([FromBody] ActivityLogFilterDto filter)
        {
            var query = _context.ActivityLogs.AsQueryable();

            // Filter by start date
            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= filter.StartDate.Value);
            }

            // Filter by end date
            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= filter.EndDate.Value);
            }

            // Filter by action type
            if (!string.IsNullOrWhiteSpace(filter.ActionType))
            {
                query = query.Where(a => a.ActionType == filter.ActionType);
            }

            // Filter by entity name
            if (!string.IsNullOrWhiteSpace(filter.EntityName))
            {
                query = query.Where(a => a.EntityName == filter.EntityName);
            }

            // Order by most recent first
            query = query.OrderByDescending(a => a.Timestamp);

            var activityLogs = await query
                .Select(a => new ActivityLogResponseDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    RecordId = a.RecordId,
                    ActionType = a.ActionType,
                    Description = a.Description,
                    PerformedBy = a.PerformedBy,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            return Ok(activityLogs);
        }

        /// <summary>
        /// Get all activity logs
        /// </summary>
        /// <returns>List of all activity logs</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> GetAllActivityLogs()
        {
            var activityLogs = await _context.ActivityLogs
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new ActivityLogResponseDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    RecordId = a.RecordId,
                    ActionType = a.ActionType,
                    Description = a.Description,
                    PerformedBy = a.PerformedBy,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            return Ok(activityLogs);
        }

        /// <summary>
        /// Get activity logs for a specific entity
        /// </summary>
        /// <param name="entityName">Name of the entity (e.g., "Item", "Client", "Quotation")</param>
        /// <param name="recordId">ID of the specific record</param>
        /// <returns>List of activity logs for the entity</returns>
        [HttpGet("{entityName}/{recordId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> GetActivityLogsForEntity(string entityName, int recordId)
        {
            var activityLogs = await _context.ActivityLogs
                .Where(a => a.EntityName == entityName && a.RecordId == recordId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new ActivityLogResponseDto
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    RecordId = a.RecordId,
                    ActionType = a.ActionType,
                    Description = a.Description,
                    PerformedBy = a.PerformedBy,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            return Ok(activityLogs);
        }
    }
}
