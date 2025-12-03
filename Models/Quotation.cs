using System;
using System.Collections.Generic;

namespace QuotationGeneratorBackEnd.Models
{
    public class Quotation
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public decimal Amount { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime QuoteDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal PartialDeposit { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "amount"; // amount or percentage
        public decimal DiscountValue { get; set; }

        // Metadata
        public string Project { get; set; } = string.Empty;
        public string AssignedUser { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Design { get; set; } = string.Empty;
        public bool InclusiveTaxes { get; set; }

        public List<QuotationItem> Items { get; set; } = new List<QuotationItem>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
