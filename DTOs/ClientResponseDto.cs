namespace quotation_generator_back_end.DTOs
{
    public class ClientResponseDto
    {
        public int ClientId { get; set; }
        public string Name { get; set; } = string.Empty;          // Client Name
        public string CompanyName { get; set; } = string.Empty;  // Company
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;  // yyyy-MM-dd
        public bool IsActive { get; set; }
    }
}
