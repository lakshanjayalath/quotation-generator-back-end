using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quotation_generator_back_end.DTOs.Dashboard;
using quotation_generator_back_end.Services;

namespace quotation_generator_back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: api/Dashboard/data
        [HttpGet("data")]
        public async Task<ActionResult<DashboardResponseDto>> GetDashboardData()
        {
            try
            {
                var result = await _dashboardService.GetDashboardDataAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load dashboard data",
                    error = ex.Message
                });
            }
        }

        // GET: api/Dashboard/quotation-pipeline?period=This Month
        [HttpGet("quotation-pipeline")]
        public async Task<ActionResult<OverviewDto>> GetQuotationPipeline(
            [FromQuery] string period = "This Month")
        {
            try
            {
                var result = await _dashboardService.GetOverviewAsync(period);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load quotation pipeline",
                    error = ex.Message
                });
            }
        }

        // GET: api/Dashboard/recent-clients?limit=5
        [HttpGet("recent-clients")]
        public async Task<ActionResult<List<RecentClientDto>>> GetRecentClients(
            [FromQuery] int limit = 5)
        {
            try
            {
                if (limit <= 0)
                    limit = 5;

                var result = await _dashboardService.GetRecentClientsAsync(limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load recent clients",
                    error = ex.Message
                });
            }
        }

        // GET: api/Dashboard/recent-activities?limit=5
        [HttpGet("recent-activities")]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities(
            [FromQuery] int limit = 5)
        {
            try
            {
                if (limit <= 0)
                    limit = 5;

                var result = await _dashboardService.GetRecentActivitiesAsync(limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load recent activities",
                    error = ex.Message
                });
            }
        }

        // GET: api/Dashboard/recent-quotations?limit=5
        [HttpGet("recent-quotations")]
        public async Task<ActionResult<List<RecentQuotationDto>>> GetRecentQuotations(
            [FromQuery] int limit = 5)
        {
            try
            {
                if (limit <= 0)
                    limit = 5;

                var result = await _dashboardService.GetRecentQuotationsAsync(limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to load recent quotations",
                    error = ex.Message
                });
            }
        }
    }
}
