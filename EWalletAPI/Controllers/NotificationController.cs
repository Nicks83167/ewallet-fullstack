using EWalletAPI.Helpers;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWalletAPI.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false)
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _notificationService.GetNotificationsAsync(userId, page, pageSize, unreadOnly);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _notificationService.MarkReadAsync(userId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _notificationService.MarkAllReadAsync(userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var userId = ClaimsHelper.GetUserId(User);
        var result = await _notificationService.DeleteNotificationAsync(userId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
