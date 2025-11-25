using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.DTOs
{
    public class UserSettingsDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Language { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? IdNumber { get; set; }

        // Password fields (optional) for updating
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }

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
    }
}
