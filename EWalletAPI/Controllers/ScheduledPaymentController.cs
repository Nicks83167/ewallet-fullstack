using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduledPaymentController : ControllerBase
{
    private readonly IScheduledPaymentService _scheduledPaymentService;

    public ScheduledPaymentController(IScheduledPaymentService scheduledPaymentService)
    {
        _scheduledPaymentService = scheduledPaymentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ScheduledPaymentDto>>>> GetScheduledPayments()
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _scheduledPaymentService.GetScheduledPaymentsAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ScheduledPaymentDto>>> CreateScheduledPayment(CreateScheduledPaymentDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _scheduledPaymentService.CreateScheduledPaymentAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("{id}/{action}")]
    public async Task<ActionResult<ApiResponseDto<object>>> UpdateScheduleStatus(Guid id, string action)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _scheduledPaymentService.UpdateScheduleStatusAsync(userId, id, action);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}