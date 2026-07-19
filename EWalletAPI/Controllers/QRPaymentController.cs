using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QRPaymentController : ControllerBase
{
    private readonly IQRPaymentService _qrPaymentService;

    public QRPaymentController(IQRPaymentService qrPaymentService)
    {
        _qrPaymentService = qrPaymentService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponseDto<QRPaymentDto>>> GenerateQR(GenerateQRRequestDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _qrPaymentService.GenerateQRAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("scan-and-pay")]
    public async Task<ActionResult<ApiResponseDto<QRPaymentDto>>> ScanAndPay(ScanQRRequestDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _qrPaymentService.ScanAndPayAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponseDto<PaginatedResultDto<QRPaymentDto>>>> GetQRHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _qrPaymentService.GetQRHistoryAsync(userId, page, pageSize);
        return Ok(result);
    }
}