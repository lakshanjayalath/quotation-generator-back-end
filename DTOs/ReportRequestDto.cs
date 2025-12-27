namespace quotation_generator_back_end.DTOs
{
    public class ReportFiltersDto
    {
        public string? Activity { get; set; }
        public string? ActionType { get; set; }
        public string? Status { get; set; }
        public string? Client { get; set; }
        public string? User { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? Search { get; set; }
        public bool IncludeDeleted { get; set; }
    }

    public class ReportOptionsDto
    {
        public string? GroupBy { get; set; }
        public string? SortBy { get; set; }
        public string? Format { get; set; } // PDF, Excel, CSV, Print
        public bool SendEmail { get; set; }
    }

    public class ReportRequestDto
    {
        public string ReportType { get; set; } = "Activity";
        // Fallback in case frontend sends action type at root
        public string? ActionType { get; set; }
        public ReportFiltersDto? Filters { get; set; }
        public ReportOptionsDto? Options { get; set; }
    }
}
