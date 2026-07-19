using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponseDto<PaginatedNotificationsDto>> GetNotificationsAsync(
        Guid userId, int page, int pageSize, bool unreadOnly)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Notifications.Where(n => n.UserId == userId);
        
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var totalCount = await query.CountAsync();
        var unreadCount = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            Icon = n.Icon,
            CreatedAt = n.CreatedAt
        });

        return ApiResponseDto<PaginatedNotificationsDto>.Ok(new PaginatedNotificationsDto
        {
            Items = dtos,
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    public async Task<ApiResponseDto<object>> MarkReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification is null)
            return ApiResponseDto<object>.Fail("Notification not found.");

        notification.IsRead = true;
        await _db.SaveChangesAsync();

        return ApiResponseDto<object>.Ok(new { }, "Notification marked as read.");
    }

    public async Task<ApiResponseDto<object>> MarkAllReadAsync(Guid userId)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications)
            n.IsRead = true;

        await _db.SaveChangesAsync();

        return ApiResponseDto<object>.Ok(new { }, $"{notifications.Count} notifications marked as read.");
    }

    public async Task<ApiResponseDto<object>> DeleteNotificationAsync(Guid userId, Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification is null)
            return ApiResponseDto<object>.Fail("Notification not found.");

        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync();

        return ApiResponseDto<object>.Ok(new { }, "Notification deleted.");
    }

    public async Task CreateNotificationAsync(Guid userId, NotificationType type, string title, string message, string? icon = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Icon = icon ?? GetDefaultIcon(type),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }

    private static string GetDefaultIcon(NotificationType type) => type switch
    {
        NotificationType.TransferSuccessful => "✅",
        NotificationType.TransferFailed => "❌",
        NotificationType.MoneyReceived => "📥",
        NotificationType.MoneySent => "📤",
        NotificationType.WalletCredited => "💰",
        NotificationType.WalletDebited => "💸",
        NotificationType.SecurityAlert => "🔒",
        NotificationType.LoginFromNewDevice => "🔐",
        _ => "🔔"
    };
}
