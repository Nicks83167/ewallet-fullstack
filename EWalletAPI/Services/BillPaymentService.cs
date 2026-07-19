using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class BillPaymentService : IBillPaymentService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly INotificationService _notificationService;

    public BillPaymentService(AppDbContext context, IWalletService walletService, INotificationService notificationService)
    {
        _context = context;
        _walletService = walletService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponseDto<PaginatedResultDto<BillPaymentDto>>> GetBillPaymentsAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var query = _context.BillPayments
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var billPayments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BillPaymentDto
                {
                    Id = x.Id,
                    Category = x.Category.ToString(),
                    Provider = x.Provider,
                    ConsumerNumber = x.ConsumerNumber,
                    Amount = x.Amount,
                    Status = x.Status.ToString(),
                    ReferenceCode = x.ReferenceCode,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var result = new PaginatedResultDto<BillPaymentDto>
            {
                Items = billPayments,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new ApiResponseDto<PaginatedResultDto<BillPaymentDto>>
            {
                Success = true,
                Message = "Bill payments retrieved successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<PaginatedResultDto<BillPaymentDto>>
            {
                Success = false,
                Message = "Failed to retrieve bill payments",
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponseDto<BillPaymentDto>> PayBillAsync(Guid userId, PayBillRequestDto request)
    {
        try
        {
            // Validate category
            if (!Enum.TryParse<BillCategory>(request.Category, out var billCategory))
            {
                return new ApiResponseDto<BillPaymentDto>
                {
                    Success = false,
                    Message = "Invalid bill category"
                };
            }

            // Check wallet balance
            var walletResult = await _walletService.GetBalanceAsync(userId);
            if (!walletResult.Success || walletResult.Data!.Balance < request.Amount)
            {
                return new ApiResponseDto<BillPaymentDto>
                {
                    Success = false,
                    Message = "Insufficient wallet balance"
                };
            }

            // Create bill payment record
            var billPayment = new BillPayment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = billCategory,
                Provider = request.Provider,
                ConsumerNumber = request.ConsumerNumber,
                Amount = request.Amount,
                Status = GatewayStatus.Processing,
                ReferenceCode = GenerateReferenceCode(),
                CreatedAt = DateTime.UtcNow
            };

            _context.BillPayments.Add(billPayment);

            // Create transaction
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = TransactionType.BillPayment,
                Amount = request.Amount,
                Direction = TransactionDirection.Out,
                Status = TransactionStatus.Pending,
                Description = $"{request.Category} bill payment - {request.Provider}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Simulate bill payment processing (in real world, this would integrate with bill payment APIs)
            var isSuccessful = SimulateBillPayment();

            if (isSuccessful)
            {
                // Deduct from wallet
                var wallet = await _context.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
                if (wallet != null)
                {
                    wallet.Balance -= request.Amount;
                    wallet.UpdatedAt = DateTime.UtcNow;
                }

                billPayment.Status = GatewayStatus.Success;
                transaction.Status = TransactionStatus.Completed;

                // Send notification
                await _notificationService.CreateNotificationAsync(
                    userId,
                    NotificationType.WalletDebited,
                    "Bill Payment Successful",
                    $"₹{request.Amount:N2} paid for {request.Category} bill to {request.Provider}",
                    "💡"
                );
            }
            else
            {
                billPayment.Status = GatewayStatus.Failed;
                transaction.Status = TransactionStatus.Failed;

                // Send notification
                await _notificationService.CreateNotificationAsync(
                    userId,
                    NotificationType.TransferFailed,
                    "Bill Payment Failed",
                    $"Payment of ₹{request.Amount:N2} to {request.Provider} failed. Please try again.",
                    "❌"
                );
            }

            await _context.SaveChangesAsync();

            var response = new BillPaymentDto
            {
                Id = billPayment.Id,
                Category = billPayment.Category.ToString(),
                Provider = billPayment.Provider,
                ConsumerNumber = billPayment.ConsumerNumber,
                Amount = billPayment.Amount,
                Status = billPayment.Status.ToString(),
                ReferenceCode = billPayment.ReferenceCode,
                CreatedAt = billPayment.CreatedAt
            };

            return new ApiResponseDto<BillPaymentDto>
            {
                Success = isSuccessful,
                Message = isSuccessful ? "Bill payment completed successfully" : "Bill payment failed",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<BillPaymentDto>
            {
                Success = false,
                Message = "Failed to process bill payment",
                Error = ex.Message
            };
        }
    }

    private static bool SimulateBillPayment()
    {
        // Simulate 95% success rate
        var random = new Random();
        return random.NextDouble() > 0.05;
    }

    private static string GenerateReferenceCode()
    {
        var random = new Random();
        return $"BP{DateTime.UtcNow:yyyyMMdd}{random.Next(100000, 999999)}";
    }
}