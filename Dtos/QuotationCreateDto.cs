using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuotationGeneratorBackEnd.Dtos
{
    public class QuotationCreateDto
    {
        [Required]
        public string Number { get; set; } = string.Empty;

        [Required]
        public string Client { get; set; } = string.Empty;

        public DateTime QuoteDate { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(7);
        public decimal PartialDeposit { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "amount";
        public decimal DiscountValue { get; set; }

        // Metadata
        public string Project { get; set; } = string.Empty;
        public string AssignedUser { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Design { get; set; } = string.Empty;
        public bool InclusiveTaxes { get; set; }

        public List<QuotationItemDto> Items { get; set; } = new List<QuotationItemDto>();
    }
}
