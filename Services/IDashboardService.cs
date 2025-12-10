using System.Collections.Generic; // Required for List<T>
using System.Threading.Tasks;
using quotation_generator_back_end.DTOs.Dashboard;

namespace quotation_generator_back_end.Services
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardDataAsync();
        
        // 🛑 FIX: Added the 'string period' parameter to match the implementation 🛑
        Task<OverviewDto> GetOverviewAsync(string period); 
        
        Task<List<RecentClientDto>> GetRecentClientsAsync(int limit = 5);
        Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int limit = 5);
        Task<List<RecentQuotationDto>> GetRecentQuotationsAsync(int limit = 5);
    }
}