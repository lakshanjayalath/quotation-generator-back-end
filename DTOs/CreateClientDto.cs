namespace quotation_generator_back_end.DTOs
{
    public class CreateClientDto
    {
        // Client Details
        public string ClientName { get; set; } = string.Empty;
        public string ClientIdNumber { get; set; } = string.Empty;
        public string ClientContactNumber { get; set; } = string.Empty;
        public string ClientAddress { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        
        // Company Details
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string AssignedUser { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string RoutingId { get; set; } = string.Empty;
        public bool ValidVat { get; set; }
        public bool TaxExempt { get; set; }
        public string Classification { get; set; } = string.Empty;
        
        // Billing Address
        public string BillingStreet { get; set; } = string.Empty;
        public string BillingSuite { get; set; } = string.Empty;
        public string BillingCity { get; set; } = string.Empty;
        public string BillingState { get; set; } = string.Empty;
        public string BillingPostalCode { get; set; } = string.Empty;
        public string BillingCountry { get; set; } = string.Empty;
        
        // Shipping Address
        public string ShippingStreet { get; set; } = string.Empty;
        public string ShippingSuite { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        
        // Contacts
        public List<ClientContactDto> Contacts { get; set; } = new();
    }
}
