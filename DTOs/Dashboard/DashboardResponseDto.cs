using System.Collections.Generic;

namespace quotation_generator_back_end.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public OverviewDto Overview { get; set; } = new();
        public List<RecentClientDto> RecentClients { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<RecentQuotationDto> RecentQuotations { get; set; } = new();
    }
}
