using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;

using quotation_generator_back_end.DTOs.Dashboard; 

namespace quotation_generator_back_end.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResponseDto> GetDashboardDataAsync()
        {
            var dashboard = new DashboardResponseDto
            {
                Overview = await GetOverviewAsync("This Month"), 
                RecentClients = await GetRecentClientsAsync(5),
                RecentActivities = await GetRecentActivitiesAsync(5),
                RecentQuotations = await GetRecentQuotationsAsync(5)
            };

            return dashboard;
        }

        public async Task<OverviewDto> GetOverviewAsync(string period)
        {
            // 1. Determine the start date
            DateTime startDate;
            Func<DateTime, string> nameSelector; 
            
            switch (period.ToLower())
            {
                case "this week":
                    // Start of the current week (Monday)
                    int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = DateTime.Today.AddDays(-1 * diff).Date;
                    // Group by day name (Mon, Tue, etc.)
                    nameSelector = date => date.ToString("ddd");
                    break;
                case "this year":
                    // Start of the current year
                    startDate = new DateTime(DateTime.Today.Year, 1, 1);
                    // Group by month name (Jan, Feb, etc.)
                    nameSelector = date => date.ToString("MMM");
                    break;
                case "this month":
                default:
                    // Start of the current month
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    // Group by day number (1, 2, 3, etc.)
                    nameSelector = date => date.Day.ToString();
                    break;
            }

            // 2. Fetch Aggregate Metrics (Filtered by period)
            var quotationsInPeriod = _context.Quotations.Where(q => q.QuoteDate >= startDate);
            
            var totalClients = await _context.Clients.CountAsync(); 
            var totalQuotations = await quotationsInPeriod.CountAsync();
            var totalItems = await _context.Items.CountAsync(); 
            
            var totalQuotationAmount = await quotationsInPeriod.SumAsync(q => (decimal?)q.Total) ?? 0m;
            
            var pendingQuotations = await quotationsInPeriod.CountAsync(q => q.Status.ToLower() == "sent"); // Assuming "Sent" is pending
            
            // 🟢 FIX 1: Use "accepted" to match the frontend status
            var approvedQuotations = await quotationsInPeriod.CountAsync(q => q.Status.ToLower() == "accepted");
            
            // 🟢 FIX 2: Use "declined" and "rejected" to cover all possibilities for the RejectedQuotations count
            var rejectedQuotations = await quotationsInPeriod.CountAsync(q => q.Status.ToLower() == "declined" || q.Status.ToLower() == "rejected" || q.Status.ToLower() == "expired");


            // 3. Fetch Time-Series Data for the Quotation Pipeline Chart (QuotationPipelineData)
            
            // Group all relevant quotes by the appropriate date interval (day, month, or year)
            var pipelinePoints = await quotationsInPeriod
                .GroupBy(q => q.QuoteDate.Date) // Group by the date part first
                .ToListAsync();

            // Project the grouped data into the QuotationDataPoint DTO
            var quotationPipelineData = pipelinePoints
                .Select(g => new QuotationDataPoint
                {
                    // Use the nameSelector delegate to format the date correctly (e.g., "Jan" or "01")
                    Name = nameSelector(g.Key), 
                    Draft = g.Count(q => q.Status.ToLower() == "draft"),
                    Sent = g.Count(q => q.Status.ToLower() == "sent"),
                    
                    // 🟢 FIX 3: Corrected statuses for chart breakdown
                    Accepted = g.Count(q => q.Status.ToLower() == "accepted"),
                    Rejected = g.Count(q => q.Status.ToLower() == "declined" || q.Status.ToLower() == "rejected" || q.Status.ToLower() == "expired")
                })
                .OrderBy(p => p.Name)
                .ToList();
            
            // 4. Return the complete OverviewDto
            return new OverviewDto
            {
                TotalClients = totalClients,
                TotalQuotations = totalQuotations,
                TotalItems = totalItems,
                TotalQuotationAmount = totalQuotationAmount,
                PendingQuotations = pendingQuotations,
                ApprovedQuotations = approvedQuotations,
                RejectedQuotations = rejectedQuotations,
                QuotationPipelineData = quotationPipelineData 
            };
        }

        // --- Other methods remain unchanged ---

        public async Task<List<RecentClientDto>> GetRecentClientsAsync(int limit = 5)
        {
            var recentClients = await _context.Clients
                .OrderByDescending(c => c.CreatedDate)
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

            return recentClients;
        }

        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 5)
        {
            var recentActivities = await _context.ActivityLogs
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

            return recentActivities;
        }

        public async Task<List<RecentQuotationDto>> GetRecentQuotationsAsync(int limit = 5)
        {
            var recentQuotations = await _context.Quotations
                .OrderByDescending(q => q.QuoteDate)
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

            return recentQuotations;
        }
    }
}