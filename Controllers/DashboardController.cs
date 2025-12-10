using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quotation_generator_back_end.DTOs.Dashboard;
using quotation_generator_back_end.Services;

namespace quotation_generator_back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route: /api/Dashboard
    [Authorize] // This secures the entire controller by default
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // --- Get Dashboard Data ---

        /// <summary>
        /// Get complete dashboard data including overview, recent clients, activities, and quotations
        /// </summary>
        [HttpGet("data")] // Endpoint: /api/Dashboard/data
        [AllowAnonymous] // 🛑 Added to allow data access for testing without a token
        public async Task<ActionResult<DashboardResponseDto>> GetDashboardData()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard data", error = ex.Message });
            }
        }

        // --- Get Quotation Pipeline Statistics ---

        /// <summary>
        /// Get quotation pipeline data for a specified period (e.g., "This Month", "This Week").
        /// This returns the OverviewDto which contains the status counts for the chart.
        /// </summary>
        // 🛑 CRITICAL CHANGE: Renaming endpoint to match frontend expectation 🛑
        [HttpGet("quotation-pipeline")] // Endpoint: /api/Dashboard/quotation-pipeline
        [AllowAnonymous] 
        public async Task<ActionResult<OverviewDto>> GetQuotationPipeline([FromQuery] string period = "This Month")
        {
            try
            {
                // The service method GetOverviewAsync now returns the Quotation-focused DTO
                var overview = await _dashboardService.GetOverviewAsync(period);
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving overview data", error = ex.Message });
            }
        }

        // --- Get Recent Clients ---
        [HttpGet("recent-clients")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecentClientDto>>> GetRecentClients([FromQuery] int limit = 5)
        {
            try
            {
                var recentClients = await _dashboardService.GetRecentClientsAsync(limit);
                return Ok(recentClients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent clients", error = ex.Message });
            }
        }

        // --- Get Recent Activities ---
        [HttpGet("recent-activities")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities([FromQuery] int limit = 5)
        {
            try
            {
                var recentActivities = await _dashboardService.GetRecentActivitiesAsync(limit);
                return Ok(recentActivities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent activities", error = ex.Message });
            }
        }

        // --- Get Recent Quotations ---
        [HttpGet("recent-quotations")]
        [AllowAnonymous] 
        public async Task<ActionResult<List<RecentQuotationDto>>> GetRecentQuotations([FromQuery] int limit = 5)
        {
            try
            {
                var recentQuotations = await _dashboardService.GetRecentQuotationsAsync(limit);
                return Ok(recentQuotations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent quotations", error = ex.Message });
            }
        }
    }
}