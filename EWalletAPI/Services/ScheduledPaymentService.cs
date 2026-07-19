using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class ScheduledPaymentService : IScheduledPaymentService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly INotificationService _notificationService;

    public ScheduledPaymentService(AppDbContext context, IWalletService walletService, INotificationService notificationService)
    {
        _context = context;
        _walletService = walletService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponseDto<IEnumerable<ScheduledPaymentDto>>> GetScheduledPaymentsAsync(Guid userId)
    {
        try
        {
            var scheduledPayments = await _context.ScheduledPayments
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ScheduledPaymentDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    PaymentType = x.PaymentType,
                    Amount = x.Amount,
                    Frequency = x.Frequency.ToString(),
                    Status = x.Status.ToString(),
                    NextRunAt = x.NextRunAt,
                    LastRunAt = x.LastRunAt,
                    RecipientEmail = x.RecipientEmail
                })
                .ToListAsync();

            return new ApiResponseDto<IEnumerable<ScheduledPaymentDto>>
            {
                Success = true,
                Message = "Scheduled payments retrieved successfully",
                Data = scheduledPayments
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<IEnumerable<ScheduledPaymentDto>>
            {
                Success = false,
                Message = "Failed to retrieve scheduled payments",
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponseDto<ScheduledPaymentDto>> CreateScheduledPaymentAsync(Guid userId, CreateScheduledPaymentDto request)
    {
        try
        {
            // Validate frequency
            if (!Enum.TryParse<ScheduleFrequency>(request.Frequency, out var frequency))
            {
                return new ApiResponseDto<ScheduledPaymentDto>
                {
                    Success = false,
                    Message = "Invalid frequency"
                };
            }

            // Calculate next run date
            var nextRunAt = CalculateNextRunDate(frequency);

            var scheduledPayment = new ScheduledPayment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                PaymentType = request.PaymentType,
                Amount = request.Amount,
                Frequency = frequency,
                Status = ScheduleStatus.Active,
                NextRunAt = nextRunAt,
                RecipientEmail = request.RecipientEmail,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.ScheduledPayments.Add(scheduledPayment);
            await _context.SaveChangesAsync();

            // Send notification
            await _notificationService.CreateNotificationAsync(
                userId,
                NotificationType.SystemAnnouncement,
                "Scheduled Payment Created",
                $"'{request.Title}' scheduled for ₹{request.Amount:N2} every {frequency.ToString().ToLower()}",
                "⏰"
            );

            var response = new ScheduledPaymentDto
            {
                Id = scheduledPayment.Id,
                Title = scheduledPayment.Title,
                PaymentType = scheduledPayment.PaymentType,
                Amount = scheduledPayment.Amount,
                Frequency = scheduledPayment.Frequency.ToString(),
                Status = scheduledPayment.Status.ToString(),
                NextRunAt = scheduledPayment.NextRunAt,
                LastRunAt = scheduledPayment.LastRunAt,
                RecipientEmail = scheduledPayment.RecipientEmail
            };

            return new ApiResponseDto<ScheduledPaymentDto>
            {
                Success = true,
                Message = "Scheduled payment created successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<ScheduledPaymentDto>
            {
                Success = false,
                Message = "Failed to create scheduled payment",
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponseDto<object>> UpdateScheduleStatusAsync(Guid userId, Guid id, string action)
    {
        try
        {
            var scheduledPayment = await _context.ScheduledPayments
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);

            if (scheduledPayment == null)
            {
                return new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Scheduled payment not found"
                };
            }

            string message;
            switch (action.ToLower())
            {
                case "pause":
                    scheduledPayment.Status = ScheduleStatus.Paused;
                    message = "Scheduled payment paused";
                    break;
                case "resume":
                    scheduledPayment.Status = ScheduleStatus.Active;
                    message = "Scheduled payment resumed";
                    break;
                case "cancel":
                    scheduledPayment.Status = ScheduleStatus.Cancelled;
                    message = "Scheduled payment cancelled";
                    break;
                case "execute":
                    await ExecuteScheduledPayment(scheduledPayment);
                    message = "Scheduled payment executed";
                    break;
                default:
                    return new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Invalid action. Use: pause, resume, cancel, or execute"
                    };
            }

            scheduledPayment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResponseDto<object>
            {
                Success = true,
                Message = message
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to update scheduled payment",
                Error = ex.Message
            };
        }
    }

    private async Task ExecuteScheduledPayment(ScheduledPayment scheduledPayment)
    {
        try
        {
            // Check if it's a transfer to another user
            if (!string.IsNullOrEmpty(scheduledPayment.RecipientEmail))
            {
                var transferRequest = new TransferRequestDto
                {
                    ReceiverEmail = scheduledPayment.RecipientEmail,
                    Amount = scheduledPayment.Amount,
                    Description = $"Scheduled Payment: {scheduledPayment.Title}"
                };

                var result = await _walletService.TransferAsync(scheduledPayment.UserId, transferRequest);
                
                if (result.Success)
                {
                    scheduledPayment.LastRunAt = DateTime.UtcNow;
                    scheduledPayment.NextRunAt = CalculateNextRunDate(scheduledPayment.Frequency);

                    await _notificationService.CreateNotificationAsync(
                        scheduledPayment.UserId,
                        NotificationType.MoneySent,
                        "Scheduled Payment Executed",
                        $"₹{scheduledPayment.Amount:N2} sent to {scheduledPayment.RecipientEmail} - {scheduledPayment.Title}",
                        "⏰"
                    );
                }
                else
                {
                    await _notificationService.CreateNotificationAsync(
                        scheduledPayment.UserId,
                        NotificationType.TransferFailed,
                        "Scheduled Payment Failed",
                        $"Failed to execute scheduled payment '{scheduledPayment.Title}' - {result.Message}",
                        "❌"
                    );
                }
            }
            else
            {
                // Handle other payment types (bills, etc.) here
                await _notificationService.CreateNotificationAsync(
                    scheduledPayment.UserId,
                    NotificationType.SystemAnnouncement,
                    "Scheduled Payment Note",
                    $"Scheduled payment '{scheduledPayment.Title}' is ready for execution",
                    "⏰"
                );
            }
        }
        catch (Exception ex)
        {
            await _notificationService.CreateNotificationAsync(
                scheduledPayment.UserId,
                NotificationType.TransferFailed,
                "Scheduled Payment Error",
                $"Error executing scheduled payment '{scheduledPayment.Title}': {ex.Message}",
                "❌"
            );
        }
    }

    private static DateTime CalculateNextRunDate(ScheduleFrequency frequency)
    {
        var now = DateTime.UtcNow;
        return frequency switch
        {
            ScheduleFrequency.Daily => now.AddDays(1),
            ScheduleFrequency.Weekly => now.AddDays(7),
            ScheduleFrequency.Monthly => now.AddMonths(1),
            ScheduleFrequency.Yearly => now.AddYears(1),
            _ => now.AddMonths(1) // Default to monthly
        };
    }
}