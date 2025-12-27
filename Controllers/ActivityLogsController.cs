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
        private readonly ILogger<ActivityLogsController> _logger;

        public ActivityLogsController(ApplicationDbContext context, ILogger<ActivityLogsController> logger)
        {
            _context = context;
            _logger = logger;
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
            _logger.LogInformation($"ActivityLogs Filter - StartDate: {filter?.StartDate}, EndDate: {filter?.EndDate}, ActionType: '{filter?.ActionType}', EntityName: '{filter?.EntityName}'");
            var query = _context.ActivityLogs.AsQueryable();

            // Parse and filter by start date
            if (!string.IsNullOrWhiteSpace(filter?.StartDate))
            {
                if (DateTime.TryParse(filter.StartDate, out var startDate))
                {
                    query = query.Where(a => a.Timestamp >= startDate);
                    _logger.LogInformation($"Applied StartDate filter: {startDate}");
                }
            }

            // Parse and filter by end date
            if (!string.IsNullOrWhiteSpace(filter?.EndDate))
            {
                if (DateTime.TryParse(filter.EndDate, out var endDate))
                {
                    // Include entire end day
                    var endOfDay = endDate.AddDays(1).AddSeconds(-1);
                    query = query.Where(a => a.Timestamp <= endOfDay);
                    _logger.LogInformation($"Applied EndDate filter: {endOfDay}");
                }
            }

            // Filter by action type (normalize values and accept synonyms)
            if (!string.IsNullOrWhiteSpace(filter?.ActionType))
            {
                var act = filter.ActionType.Trim().ToLowerInvariant();
                _logger.LogInformation($"Processing ActionType filter: '{act}'");
                
                if (act == "all")
                {
                    _logger.LogInformation("ActionType is 'all', no filter applied");
                }
                else if (act == "created" || act == "create")
                {
                    query = query.Where(a => a.ActionType.ToLower() == "create");
                    _logger.LogInformation("Applied ActionType filter: Create");
                }
                else if (act == "updated" || act == "update")
                {
                    query = query.Where(a => a.ActionType.ToLower() == "update");
                    _logger.LogInformation("Applied ActionType filter: Update");
                }
                else if (act == "deleted" || act == "delete")
                {
                    query = query.Where(a => a.ActionType.ToLower() == "delete");
                    _logger.LogInformation("Applied ActionType filter: Delete");
                }
                else
                {
                    // any other action (e.g., login) matches case-insensitively
                    query = query.Where(a => a.ActionType.ToLower() == act);
                    _logger.LogInformation($"Applied ActionType filter: {act}");
                }
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
