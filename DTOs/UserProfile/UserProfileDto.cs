namespace quotation_generator_back_end.DTOs.UserProfile
{
    /// <summary>
    /// DTO for returning user profile data
    /// </summary>
    public class UserProfileDto
    {
        public int Id { get; set; }
        
        // Basic Information
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Role { get; set; }

        // Address
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        // Preferences
        public string? Language { get; set; }
        public string? PreferredContactMethod { get; set; }
        public bool Notifications { get; set; }

        // Notes
        public string? Notes { get; set; }

        // Quotation Summary
        public QuotationSummaryDto? QuotationSummary { get; set; }

        // Recent Quotations
        public List<QuotationHistoryDto>? Quotations { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Summary of user's quotations
    /// </summary>
    public class QuotationSummaryDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public decimal TotalValue { get; set; }
    }

    /// <summary>
    /// DTO for quotation history in profile
    /// </summary>
    public class QuotationHistoryDto
    {
        public int Id { get; set; }
        public string? QuoteNo { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
    }
}
