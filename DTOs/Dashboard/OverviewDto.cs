using System.Collections.Generic;
// Use the specific DTO for the chart data
using quotation_generator_back_end.DTOs.Dashboard; 

namespace quotation_generator_back_end.DTOs.Dashboard
{
    /// <summary>
    /// Data Transfer Object for the Quotation Overview dashboard view.
    /// </summary>
    public class OverviewDto
    {
        // High-level summary KPIs (Quotation Focused)
        public int TotalClients { get; set; }
        public int TotalQuotations { get; set; }
        public int TotalItems { get; set; }
        
        // The total value of all quotations created (regardless of status)
        public decimal TotalQuotationAmount { get; set; } 
        
        // Count of quotations currently awaiting client response
        public int PendingQuotations { get; set; }
        
        // Count of quotations that have been accepted by the client
        public int ApprovedQuotations { get; set; }
        
        // Count of quotations that have been rejected/lost
        public int RejectedQuotations { get; set; }

        // 🛑 This property holds the structured data for the Quotation Pipeline Bar Chart 🛑
        public List<QuotationDataPoint> QuotationPipelineData { get; set; } = new List<QuotationDataPoint>();
    }
}