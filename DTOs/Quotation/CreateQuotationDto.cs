using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.DTOs.Quotation
{
    public class CreateQuotationDto
    {
        [Required]
        public string QuoteNumber { get; set; } = string.Empty;

        public string? PoNumber { get; set; }

        public int? ClientId { get; set; }

        public string? ClientName { get; set; }

        [Required]
        public DateTime QuoteDate { get; set; }

        public DateTime? ValidUntil { get; set; }

        public decimal PartialDeposit { get; set; }

        public string DiscountType { get; set; } = "amount";

        public decimal Discount { get; set; }

        // Settings
        public string? Project { get; set; }
        public string? AssignedUser { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Vendor { get; set; }
        public string? Design { get; set; }
        public bool InclusiveTaxes { get; set; }

        public List<CreateQuotationItemDto> Items { get; set; } = new List<CreateQuotationItemDto>();
    }

    public class CreateQuotationItemDto
    {
        [Required]
        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal UnitCost { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
