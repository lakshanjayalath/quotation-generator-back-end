namespace quotation_generator_back_end.DTOs.Dashboard
{
    public class RecentClientDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientContactNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
