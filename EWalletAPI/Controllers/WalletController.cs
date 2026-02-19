using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
[Produces("application/json")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>Get the current user's wallet balance.</summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(ApiResponseDto<WalletBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance()
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _walletService.GetBalanceAsync(userId);

        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Deposit money into the current user's wallet.</summary>
    [HttpPost("add-money")]
    [ProducesResponseType(typeof(ApiResponseDto<WalletOperationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddMoney([FromBody] AddMoneyRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(ApiResponseDto<object>.Fail("Validation failed.", errors));
        }

        var userId = ClaimsHelper.GetUserId(User);
        var result = await _walletService.AddMoneyAsync(userId, request);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Transfer funds to another user by email.</summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponseDto<WalletOperationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(ApiResponseDto<object>.Fail("Validation failed.", errors));
        }

        var userId = ClaimsHelper.GetUserId(User);
        var result = await _walletService.TransferAsync(userId, request);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
