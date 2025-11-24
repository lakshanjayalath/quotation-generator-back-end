namespace quotation_generator_back_end.DTOs
{
    public class UpdateClientDto
    {
        public string? ClientName { get; set; }
        public string? ClientIdNumber { get; set; }
        public string? ClientContactNumber { get; set; }
        public string? ClientAddress { get; set; }
        public string? ClientEmail { get; set; }
        public string? Name { get; set; }
        public string? Number { get; set; }
        public string? Group { get; set; }
        public string? AssignedUser { get; set; }
        public string? IdNumber { get; set; }
        public string? VatNumber { get; set; }
        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? RoutingId { get; set; }
        public bool? ValidVat { get; set; }
        public bool? TaxExempt { get; set; }
        public string? Classification { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingSuite { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
        public string? ShippingStreet { get; set; }
        public string? ShippingSuite { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingPostalCode { get; set; }
        public string? ShippingCountry { get; set; }
        public bool? IsActive { get; set; }
        public List<ClientContactDto>? Contacts { get; set; }
    }
}
