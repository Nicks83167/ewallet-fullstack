using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RechargeController : ControllerBase
{
    private readonly IRechargeService _rechargeService;

    public RechargeController(IRechargeService rechargeService)
    {
        _rechargeService = rechargeService;
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponseDto<PaginatedResultDto<RechargeRecordDto>>>> GetRechargeHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _rechargeService.GetRechargeHistoryAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<RechargeRecordDto>>> Recharge(RechargeRequestDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _rechargeService.RechargeAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}