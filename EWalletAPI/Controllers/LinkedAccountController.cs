using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LinkedAccountController : ControllerBase
{
    private readonly ILinkedAccountService _linkedAccountService;

    public LinkedAccountController(ILinkedAccountService linkedAccountService)
    {
        _linkedAccountService = linkedAccountService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<LinkedAccountDto>>>> GetLinkedAccounts()
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _linkedAccountService.GetLinkedAccountsAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<LinkedAccountDto>>> CreateLinkedAccount(CreateLinkedAccountDto request)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _linkedAccountService.CreateLinkedAccountAsync(userId, request);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteLinkedAccount(Guid id)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _linkedAccountService.DeleteLinkedAccountAsync(userId, id);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("{id}/set-default")]
    public async Task<ActionResult<ApiResponseDto<object>>> SetDefaultLinkedAccount(Guid id)
    {
        var userId = ClaimsHelper.GetUserId(HttpContext.User);
        var result = await _linkedAccountService.SetDefaultLinkedAccountAsync(userId, id);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }
}