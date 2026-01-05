namespace quotation_generator_back_end.DTOs.Dashboard
{
    /// <summary>
    /// Represents a single aggregated point of quotation data, typically used for charting
    /// the quotation pipeline by status over a time period (day, week, month).
    /// </summary>
    public class QuotationDataPoint
    {
        // 🛑 FIX for CS8618: Initialize the non-nullable string to string.Empty 🛑
        public string Name { get; set; } = string.Empty; 
        
        // Count of quotations currently in Draft status
        public int Draft { get; set; }
        
        // Count of quotations that have been sent to the client
        public int Sent { get; set; }
        
        // Count of quotations that the client has formally approved/accepted
        public int Accepted { get; set; }
        
        // Count of quotations that have been formally rejected or marked as lost
        public int Rejected { get; set; }

        public int Expired { get; set; }
    }
}