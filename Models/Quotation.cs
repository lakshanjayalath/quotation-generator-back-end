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

        // 🔹 Client relationship
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        [MaxLength(200)]
        public string? ClientName { get; set; }

        public DateTime QuoteDate { get; set; }
        public DateTime? ValidUntil { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PartialDeposit { get; set; }

        [MaxLength(20)]
        public string DiscountType { get; set; } = "amount"; // amount | percentage

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
        public string Status { get; set; } = "Draft"; 
        // Draft, Sent, Accepted, Expired, Declined

        // 🔹 Settings
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

        // 🔐 OWNERSHIP (CRITICAL FOR USER ISOLATION)

        /// <summary>
        /// User ID of the person who created this quotation.
        /// Used to restrict visibility for normal users.
        /// </summary>
        public int? CreatedById { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public User? CreatedBy { get; set; }

        /// <summary>
        /// Email snapshot of creator (optional but useful for audits)
        /// </summary>
        [MaxLength(200)]
        public string? CreatedByEmail { get; set; }

        // 🔹 Quotation Items
        public List<QuotationItem> Items { get; set; } = new();

        // 🔹 Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class QuotationItem
    {
        public int Id { get; set; }

        // 🔹 Quotation relationship
        public int QuotationId { get; set; }
        public Quotation? Quotation { get; set; }

        // 🔹 Item relationship
        public int? ItemId { get; set; }
        public Item? Item { get; set; }

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
