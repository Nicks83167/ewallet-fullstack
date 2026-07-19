using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>Get comprehensive dashboard analytics for the current user.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponseDto<DashboardAnalyticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDashboardAnalytics()
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _analyticsService.GetDashboardAnalyticsAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Generate detailed report for a specific period.</summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(ApiResponseDto<ReportOverviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateReport(
        [FromQuery] string period = "monthly",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _analyticsService.GenerateReportAsync(userId, period, startDate, endDate);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
