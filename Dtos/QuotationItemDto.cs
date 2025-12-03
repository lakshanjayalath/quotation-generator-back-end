using System.ComponentModel.DataAnnotations;

namespace QuotationGeneratorBackEnd.Dtos
{
    public class QuotationItemDto
    {
        public string Item { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public decimal Quantity { get; set; }
    }
}
