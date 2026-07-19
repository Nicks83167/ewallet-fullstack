using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillPaymentController : ControllerBase
{
    private readonly IBillPaymentService _billPaymentService;

    public BillPaymentController(IBillPaymentService billPaymentService)
    {
        _billPaymentService = billPaymentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PaginatedResultDto<BillPaymentDto>>>> GetBillPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _billPaymentService.GetBillPaymentsAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost("pay")]
    public async Task<ActionResult<ApiResponseDto<BillPaymentDto>>> PayBill(PayBillRequestDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _billPaymentService.PayBillAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}