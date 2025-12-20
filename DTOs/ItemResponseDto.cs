namespace quotation_generator_back_end.DTOs
{
    public class ItemResponseDto
    {
        public int Id { get; set; }
        public string Item { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
