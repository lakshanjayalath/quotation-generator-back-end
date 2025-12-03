using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.DTOs.Quotation
{
    public class UpdateQuotationDto
    {
        public string? QuoteNumber { get; set; }

        public string? PoNumber { get; set; }

        public int? ClientId { get; set; }

        public string? ClientName { get; set; }

        public DateTime? QuoteDate { get; set; }

        public DateTime? ValidUntil { get; set; }

        public decimal? PartialDeposit { get; set; }

        public string? DiscountType { get; set; }

        public decimal? Discount { get; set; }

        public string? Status { get; set; }

        // Settings
        public string? Project { get; set; }
        public string? AssignedUser { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Vendor { get; set; }
        public string? Design { get; set; }
        public bool? InclusiveTaxes { get; set; }

        public List<UpdateQuotationItemDto>? Items { get; set; }
    }

    public class UpdateQuotationItemDto
    {
        public int? Id { get; set; } // If null, it's a new item

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal UnitCost { get; set; }

        public int Quantity { get; set; }
    }
}
