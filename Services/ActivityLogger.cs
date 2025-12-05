using quotation_generator_back_end.Data;
using quotation_generator_back_end.Models;

namespace quotation_generator_back_end.Services
{
    public interface IActivityLogger
    {
        Task LogAsync(string entityName, int recordId, string actionType, string? description = null, string? performedBy = null);
    }

    public class ActivityLogger : IActivityLogger
    {
        private readonly ApplicationDbContext _context;

        // Sri Lankan Time Zone (UTC+5:30)
        private static readonly TimeSpan SriLankaOffset = TimeSpan.FromHours(5.5);

        public ActivityLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string entityName, int recordId, string actionType, string? description = null, string? performedBy = null)
        {
            // Get current time in Sri Lankan timezone (UTC+5:30)
            var sriLankaTime = DateTime.UtcNow.Add(SriLankaOffset);

            var log = new ActivityLog
            {
                EntityName = entityName,
                RecordId = recordId,
                ActionType = actionType,
                Description = description,
                PerformedBy = performedBy,
                Timestamp = sriLankaTime
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
