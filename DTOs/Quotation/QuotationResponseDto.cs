using System;
using System.Collections.Generic;

namespace quotation_generator_back_end.DTOs.Quotation
{
    public class QuotationResponseDto
    {
        public int Id { get; set; }
        public string QuoteNumber { get; set; } = string.Empty;
        public string? PoNumber { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public DateTime QuoteDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal PartialDeposit { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        // Settings
        public string? Project { get; set; }
        public string? AssignedUser { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Vendor { get; set; }
        public string? Design { get; set; }
        public bool InclusiveTaxes { get; set; }

        public List<QuotationItemResponseDto> Items { get; set; } = new List<QuotationItemResponseDto>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class QuotationItemResponseDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitCost { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class QuotationListResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string QuoteNumber { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public decimal Total { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime QuoteDate { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
