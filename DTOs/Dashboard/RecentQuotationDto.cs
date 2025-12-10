namespace quotation_generator_back_end.DTOs.Dashboard
{
    public class RecentQuotationDto
    {
        public int Id { get; set; }
        public string QuoteNumber { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public DateTime QuoteDate { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
    }
}
