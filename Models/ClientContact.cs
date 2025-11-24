using System.Text.Json.Serialization;

namespace quotation_generator_back_end.Models
{
    public class ClientContact
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool AddToInvoices { get; set; }
        
        // Foreign key
        public int ClientId { get; set; }
        
        [JsonIgnore]
        public Client Client { get; set; } = null!;
    }
}
