using Microsoft.AspNetCore.Mvc;
using quotation_generator_back_end.DTOs;
using quotation_generator_back_end.Services;
using System.Data;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IReportExportService _exportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, IReportExportService exportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _exportService = exportService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ReportRequestDto request)
        {
            try
            {
                var rows = await _reportService.GenerateReportAsync(request);
                return Ok(rows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] ReportRequestDto request)
        {
            try
            {
                var dt = await _reportService.GenerateReportTableAsync(request);
                var format = (request.Options?.Format ?? "CSV").ToUpperInvariant();
                byte[] fileBytes;
                string filenameBase = $"report_{request.ReportType}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                string contentType;
                string ext;

                switch (format)
                {
                    case "EXCEL":
                    case "XLSX":
                        fileBytes = _exportService.ExportExcel(dt);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        ext = "xlsx";
                        break;

                    case "PDF":
                        fileBytes = _exportService.ExportPdf(dt, $"{request.ReportType} Report");
                        contentType = "application/pdf";
                        ext = "pdf";
                        break;

                    case "CSV":
                    default:
                        fileBytes = _exportService.ExportCsv(dt);
                        contentType = "text/csv";
                        ext = "csv";
                        break;
                }

                return File(fileBytes, contentType, $"{filenameBase}.{ext}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
