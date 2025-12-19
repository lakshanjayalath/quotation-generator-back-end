using System;
using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? FirstName { get; set; }

        [MaxLength(200)]
        public string? LastName { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string Role { get; set; } = "User";  // "Admin" or "User"

        [MaxLength(50)]
        public string? Language { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(1000)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? IdNumber { get; set; }

        [MaxLength(200)]
        public string? PasswordHash { get; set; }

        public bool TwoFactorAuth { get; set; }
        public bool LoginNotification { get; set; }
        public bool TaskAssignNotification { get; set; }
        public bool DisableRecurringPaymentNotification { get; set; }

        public string? AllEvents { get; set; }
        public string? InvoiceCreated { get; set; }
        public string? InvoiceSent { get; set; }
        public string? QuoteCreated { get; set; }
        public string? QuoteSent { get; set; }
        public string? QuoteView { get; set; }
        public string? PaymentDetails { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
