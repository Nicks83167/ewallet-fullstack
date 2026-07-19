using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class RechargeService : IRechargeService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly INotificationService _notificationService;

    public RechargeService(AppDbContext context, IWalletService walletService, INotificationService notificationService)
    {
        _context = context;
        _walletService = walletService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponseDto<PaginatedResultDto<RechargeRecordDto>>> GetRechargeHistoryAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var query = _context.RechargeRecords
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var recharges = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RechargeRecordDto
                {
                    Id = x.Id,
                    RechargeType = x.RechargeType.ToString(),
                    Operator = x.Operator,
                    MobileNumber = x.MobileNumber,
                    PlanName = x.PlanName,
                    Amount = x.Amount,
                    Status = x.Status.ToString(),
                    IsFavourite = x.IsFavourite,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var result = new PaginatedResultDto<RechargeRecordDto>
            {
                Items = recharges,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new ApiResponseDto<PaginatedResultDto<RechargeRecordDto>>
            {
                Success = true,
                Message = "Recharge history retrieved successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<PaginatedResultDto<RechargeRecordDto>>
            {
                Success = false,
                Message = "Failed to retrieve recharge history",
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponseDto<RechargeRecordDto>> RechargeAsync(Guid userId, RechargeRequestDto request)
    {
        try
        {
            // Validate recharge type
            if (!Enum.TryParse<RechargeType>(request.RechargeType, out var rechargeType))
            {
                return new ApiResponseDto<RechargeRecordDto>
                {
                    Success = false,
                    Message = "Invalid recharge type"
                };
            }

            // Check wallet balance
            var walletResult = await _walletService.GetBalanceAsync(userId);
            if (!walletResult.Success || walletResult.Data!.Balance < request.Amount)
            {
                return new ApiResponseDto<RechargeRecordDto>
                {
                    Success = false,
                    Message = "Insufficient wallet balance"
                };
            }

            // Validate mobile number
            if (!IsValidMobileNumber(request.MobileNumber))
            {
                return new ApiResponseDto<RechargeRecordDto>
                {
                    Success = false,
                    Message = "Invalid mobile number format"
                };
            }

            // Create recharge record
            var recharge = new RechargeRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RechargeType = rechargeType,
                Operator = request.Operator,
                MobileNumber = request.MobileNumber,
                PlanName = request.PlanName,
                Amount = request.Amount,
                Status = GatewayStatus.Processing,
                CreatedAt = DateTime.UtcNow
            };

            _context.RechargeRecords.Add(recharge);

            // Create transaction
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = TransactionType.Recharge,
                Amount = request.Amount,
                Direction = TransactionDirection.Out,
                Status = TransactionStatus.Pending,
                Description = $"{request.RechargeType} recharge - {request.Operator} ({request.MobileNumber})",
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Simulate recharge processing
            var isSuccessful = SimulateRecharge();

            if (isSuccessful)
            {
                // Deduct from wallet
                var wallet = await _context.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
                if (wallet != null)
                {
                    wallet.Balance -= request.Amount;
                    wallet.UpdatedAt = DateTime.UtcNow;
                }

                recharge.Status = GatewayStatus.Success;
                transaction.Status = TransactionStatus.Completed;

                // Check if this number should be marked as favourite (if recharged multiple times)
                var rechargeCount = await _context.RechargeRecords
                    .CountAsync(x => x.UserId == userId && x.MobileNumber == request.MobileNumber && x.Status == GatewayStatus.Success);

                if (rechargeCount >= 3)
                {
                    recharge.IsFavourite = true;
                }

                // Send notification
                await _notificationService.CreateNotificationAsync(
                    userId,
                    NotificationType.WalletDebited,
                    "Recharge Successful",
                    $"₹{request.Amount:N2} recharge completed for {request.MobileNumber} ({request.Operator})",
                    "📱"
                );
            }
            else
            {
                recharge.Status = GatewayStatus.Failed;
                transaction.Status = TransactionStatus.Failed;

                // Send notification
                await _notificationService.CreateNotificationAsync(
                    userId,
                    NotificationType.RechargeReminder,
                    "Recharge Failed",
                    $"Recharge of ₹{request.Amount:N2} for {request.MobileNumber} failed. Please try again.",
                    "❌"
                );
            }

            await _context.SaveChangesAsync();

            var response = new RechargeRecordDto
            {
                Id = recharge.Id,
                RechargeType = recharge.RechargeType.ToString(),
                Operator = recharge.Operator,
                MobileNumber = recharge.MobileNumber,
                PlanName = recharge.PlanName,
                Amount = recharge.Amount,
                Status = recharge.Status.ToString(),
                IsFavourite = recharge.IsFavourite,
                CreatedAt = recharge.CreatedAt
            };

            return new ApiResponseDto<RechargeRecordDto>
            {
                Success = isSuccessful,
                Message = isSuccessful ? "Recharge completed successfully" : "Recharge failed",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<RechargeRecordDto>
            {
                Success = false,
                Message = "Failed to process recharge",
                Error = ex.Message
            };
        }
    }

    private static bool SimulateRecharge()
    {
        // Simulate 97% success rate
        var random = new Random();
        return random.NextDouble() > 0.03;
    }

    private static bool IsValidMobileNumber(string mobileNumber)
    {
        // Indian mobile number validation (10 digits starting with 6-9)
        if (string.IsNullOrEmpty(mobileNumber))
            return false;

        // Remove any spaces, hyphens, or parentheses
        var cleanedNumber = mobileNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // Check if it starts with +91 and remove it
        if (cleanedNumber.StartsWith("+91"))
            cleanedNumber = cleanedNumber[3..];

        // Check if it's exactly 10 digits and starts with 6, 7, 8, or 9
        return cleanedNumber.Length == 10 && 
               cleanedNumber.All(char.IsDigit) && 
               "6789".Contains(cleanedNumber[0]);
    }
}