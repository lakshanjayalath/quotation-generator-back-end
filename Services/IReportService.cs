using System.Data;

namespace quotation_generator_back_end.Services
{
    public interface IReportService
    {
        Task<List<Dictionary<string, object>>> GenerateReportAsync( DTOs.ReportRequestDto request );
        Task<DataTable> GenerateReportTableAsync( DTOs.ReportRequestDto request );
    }
}
