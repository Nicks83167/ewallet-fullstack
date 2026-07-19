using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<CurrencyDto>>>> GetCurrencies()
    {
        var result = await _currencyService.GetCurrenciesAsync();
        return Ok(result);
    }

    [HttpGet("exchange-rates")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ExchangeRateDto>>>> GetExchangeRates()
    {
        var result = await _currencyService.GetExchangeRatesAsync();
        return Ok(result);
    }

    [HttpPost("convert")]
    public async Task<ActionResult<ApiResponseDto<CurrencyConvertResponseDto>>> ConvertCurrency(CurrencyConvertRequestDto request)
    {
        var result = await _currencyService.ConvertCurrencyAsync(request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("switch-wallet-currency")]
    public async Task<ActionResult<ApiResponseDto<WalletBalanceDto>>> SwitchWalletCurrency(SwitchWalletCurrencyRequestDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _currencyService.SwitchWalletCurrencyAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}