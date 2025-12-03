using System;
using System.Collections.Generic;

namespace QuotationGeneratorBackEnd.Dtos
{
    public class QuotationReadDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime QuoteDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public List<QuotationItemDto> Items { get; set; } = new List<QuotationItemDto>();
    }
}
