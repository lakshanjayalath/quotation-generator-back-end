namespace quotation_generator_back_end.DTOs
{
    public class NextClientIdDto
    {
        public int NextId { get; set; }
        public string FormattedClientId { get; set; } = string.Empty;
        public string Message { get; set; } = "Next Client ID available";
    }
}
