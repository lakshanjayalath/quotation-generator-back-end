using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quotation_generator_back_end.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string QuoteNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PoNumber { get; set; }

        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        [MaxLength(200)]
        public string? ClientName { get; set; }

        public DateTime QuoteDate { get; set; }
        public DateTime? ValidUntil { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PartialDeposit { get; set; }

        [MaxLength(20)]
        public string DiscountType { get; set; } = "amount"; // "amount" or "percentage"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Expired, Declined

        // Settings
        [MaxLength(200)]
        public string? Project { get; set; }

        [MaxLength(200)]
        public string? AssignedUser { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? ExchangeRate { get; set; }

        [MaxLength(200)]
        public string? Vendor { get; set; }

        [MaxLength(200)]
        public string? Design { get; set; }

        public bool InclusiveTaxes { get; set; }

        // Navigation property for items
        public List<QuotationItem> Items { get; set; } = new List<QuotationItem>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class QuotationItem
    {
        public int Id { get; set; }

        public int QuotationId { get; set; }
        public Quotation? Quotation { get; set; }

        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }
}
