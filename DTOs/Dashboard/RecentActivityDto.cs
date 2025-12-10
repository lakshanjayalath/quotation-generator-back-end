namespace quotation_generator_back_end.DTOs.Dashboard
{
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PerformedBy { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
