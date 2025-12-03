using System;

namespace QuotationGeneratorBackEnd.Models
{
    public class QuotationItem
    {
        public int Id { get; set; }
        public string Item { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
