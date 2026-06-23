using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponseDto<object>.Fail("Validation failed.", errors));
        }

        var result = await _authService.RegisterAsync(request);
        if (!result.Success) return Conflict(result);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>Authenticate and receive a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponseDto<object>.Fail("Validation failed.", errors));
        }

        var result = await _authService.LoginAsync(request);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    /// <summary>Get the current user's profile.</summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _authService.GetProfileAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update the current user's full name.</summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponseDto<object>.Fail("Validation failed.", errors));
        }

        var userId = ClaimsHelper.GetUserId(User);
        var result = await _authService.UpdateProfileAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Change the current user's password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponseDto<object>.Fail("Validation failed.", errors));
        }

        var userId = ClaimsHelper.GetUserId(User);
        var result = await _authService.ChangePasswordAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
