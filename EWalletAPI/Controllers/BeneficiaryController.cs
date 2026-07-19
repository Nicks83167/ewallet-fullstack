using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BeneficiaryController : ControllerBase
{
    private readonly IBeneficiaryService _beneficiaryService;

    public BeneficiaryController(IBeneficiaryService beneficiaryService)
    {
        _beneficiaryService = beneficiaryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<BeneficiaryDto>>>> GetBeneficiaries(
        [FromQuery] string? search = null)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _beneficiaryService.GetBeneficiariesAsync(userId, search);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<BeneficiaryDto>>> CreateBeneficiary(CreateBeneficiaryDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _beneficiaryService.CreateBeneficiaryAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<BeneficiaryDto>>> UpdateBeneficiary(Guid id, CreateBeneficiaryDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _beneficiaryService.UpdateBeneficiaryAsync(userId, id, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteBeneficiary(Guid id)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _beneficiaryService.DeleteBeneficiaryAsync(userId, id);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("{id}/toggle-favourite")]
    public async Task<ActionResult<ApiResponseDto<object>>> ToggleFavourite(Guid id)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _beneficiaryService.ToggleFavouriteAsync(userId, id);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}