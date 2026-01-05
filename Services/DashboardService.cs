using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs.Dashboard;
using System.Security.Claims;

namespace quotation_generator_back_end.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardResponseDto> GetDashboardDataAsync()
        {
            return new DashboardResponseDto
            {
                Overview = await GetOverviewAsync("This Month"),
                RecentClients = await GetRecentClientsAsync(5),
                RecentActivities = await GetRecentActivitiesAsync(5),
                RecentQuotations = await GetRecentQuotationsAsync(5)
            };
        }

        // ---------------- OVERVIEW ----------------

        public async Task<OverviewDto> GetOverviewAsync(string period)
        {
            DateTime startDate;
            Func<DateTime, string> nameSelector;

            switch (period.ToLowerInvariant())
            {
                case "this week":
                    int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = DateTime.Today.AddDays(-diff);
                    nameSelector = d => d.ToString("ddd");
                    break;

                case "this year":
                    startDate = new DateTime(DateTime.Today.Year, 1, 1);
                    nameSelector = d => d.ToString("MMM");
                    break;

                default:
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    nameSelector = d => d.Day.ToString();
                    break;
            }

            var isAdmin = IsAdmin();
            var userId = GetCurrentUserId();
            var email = NormalizeEmail(GetCurrentUserEmail());

            var quotations = _context.Quotations
                .Where(q => q.QuoteDate >= startDate)
                .AsQueryable();

            if (!isAdmin && (userId.HasValue || email != null))
            {
                quotations = quotations.Where(q =>
                    (userId.HasValue && q.CreatedById == userId)
                    || (email != null && q.CreatedByEmail != null && q.CreatedByEmail.ToLower() == email)
                    || (email != null && q.AssignedUser != null && q.AssignedUser.ToLower() == email));
            }

            var totalClients = await _context.Clients.CountAsync();
            var totalQuotations = await quotations.CountAsync();
            var totalItems = await _context.Items.CountAsync();

            var totalQuotationAmount =
                await quotations.SumAsync(q => (decimal?)q.Total) ?? 0m;

            var pending = await quotations.CountAsync(q => q.Status.ToLower() == "sent");
            var approved = await quotations.CountAsync(q => q.Status.ToLower() == "accepted");
            var rejected = await quotations.CountAsync(q =>
                q.Status.ToLower() == "declined" || q.Status.ToLower() == "rejected");

            var grouped = await quotations
                .GroupBy(q => q.QuoteDate.Date)
                .ToListAsync();

            var pipeline = grouped
                .Select(g => new QuotationDataPoint
                {
                    Name = nameSelector(g.Key),
                    Draft = g.Count(x => x.Status.ToLower() == "draft"),
                    Sent = g.Count(x => x.Status.ToLower() == "sent"),
                    Accepted = g.Count(x => x.Status.ToLower() == "accepted"),
                    Rejected = g.Count(x => x.Status.ToLower() == "declined"),
                    Expired = g.Count(x => x.Status.ToLower() == "expired")
                })
                .OrderBy(x => x.Name)
                .ToList();

            return new OverviewDto
            {
                TotalClients = totalClients,
                TotalQuotations = totalQuotations,
                TotalItems = totalItems,
                TotalQuotationAmount = totalQuotationAmount,
                PendingQuotations = pending,
                ApprovedQuotations = approved,
                RejectedQuotations = rejected,
                QuotationPipelineData = pipeline
            };
        }

        // ---------------- RECENT CLIENTS ----------------

        public async Task<List<RecentClientDto>> GetRecentClientsAsync(int limit = 5)
        {
            var isAdmin = IsAdmin();
            var email = NormalizeEmail(GetCurrentUserEmail());

            var query = _context.Clients.AsQueryable();

            if (!isAdmin && email != null)
            {
                query = query.Where(c =>
                    c.AssignedUser != null &&
                    c.AssignedUser.ToLower() == email);
            }

            return await query
                .OrderByDescending(c => c.CreatedDate) // 🔴 FIXED
                .Take(limit)
                .Select(c => new RecentClientDto
                {
                    Id = c.Id,
                    ClientName = c.ClientName,
                    ClientEmail = c.ClientEmail,
                    ClientContactNumber = c.ClientContactNumber,
                    City = c.BillingCity ?? string.Empty,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync();
        }

        // ---------------- RECENT ACTIVITIES ----------------

        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 5)
        {
            var isAdmin = IsAdmin();
            var userId = GetCurrentUserId();
            var email = NormalizeEmail(GetCurrentUserEmail());

            var query = _context.ActivityLogs.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(a =>
                    (userId.HasValue && a.UserId == userId)
                    || (email != null && a.PerformedBy != null && a.PerformedBy.ToLower() == email)
                    || (email != null && a.PerformedByEmail != null && a.PerformedByEmail.ToLower() == email));
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .Select(a => new RecentActivityDto
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
        }

        // ---------------- RECENT QUOTATIONS ----------------

        public async Task<List<RecentQuotationDto>> GetRecentQuotationsAsync(int limit = 5)
        {
            var isAdmin = IsAdmin();
            var userId = GetCurrentUserId();
            var email = NormalizeEmail(GetCurrentUserEmail());

            var query = _context.Quotations.AsQueryable();

            if (!isAdmin && (userId.HasValue || email != null))
            {
                query = query.Where(q =>
                    (userId.HasValue && q.CreatedById == userId)
                    || (email != null && q.CreatedByEmail != null && q.CreatedByEmail.ToLower() == email)
                    || (email != null && q.AssignedUser != null && q.AssignedUser.ToLower() == email));
            }

            return await query
                .OrderByDescending(q => q.QuoteDate) // 🔴 FIXED
                .Take(limit)
                .Select(q => new RecentQuotationDto
                {
                    Id = q.Id,
                    QuoteNumber = q.QuoteNumber,
                    ClientName = q.ClientName,
                    QuoteDate = q.QuoteDate,
                    Total = q.Total,
                    Status = q.Status,
                    ValidUntil = q.ValidUntil
                })
                .ToListAsync();
        }

        // ---------------- HELPERS ----------------

        private int? GetCurrentUserId()
        {
            var value = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(value, out var id) ? id : null;
        }

        private string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.Email)?.Value
                ?? _httpContextAccessor.HttpContext?.User
                ?.FindFirst("email")?.Value;
        }

        private bool IsAdmin()
        {
            var role = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.Role)?.Value;

            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeEmail(string? email)
        {
            return string.IsNullOrWhiteSpace(email)
                ? null
                : email.Trim().ToLowerInvariant();
        }
    }
}
