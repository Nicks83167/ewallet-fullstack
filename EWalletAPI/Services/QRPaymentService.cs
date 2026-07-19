using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace EWalletAPI.Services;

public class QRPaymentService : IQRPaymentService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly INotificationService _notificationService;

    public QRPaymentService(AppDbContext context, IWalletService walletService, INotificationService notificationService)
    {
        _context = context;
        _walletService = walletService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponseDto<QRPaymentDto>> GenerateQRAsync(Guid userId, GenerateQRRequestDto request)
    {
        try
        {
            // Get user info
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResponseDto<QRPaymentDto>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var qrPayment = new QRPayment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PaymentType = QRPaymentType.Receive,
                Amount = request.Amount,
                Description = request.Description ?? "Payment Request",
                Status = GatewayStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Generate QR code data
            var qrData = new
            {
                id = qrPayment.Id,
                userId = userId,
                amount = request.Amount,
                description = request.Description,
                merchantName = user.FullName,
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            qrPayment.QrCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(qrData)));

            _context.QRPayments.Add(qrPayment);
            await _context.SaveChangesAsync();

            var response = new QRPaymentDto
            {
                Id = qrPayment.Id,
                PaymentType = qrPayment.PaymentType.ToString(),
                QrCode = qrPayment.QrCode,
                Amount = qrPayment.Amount,
                Status = qrPayment.Status.ToString(),
                MerchantName = user.FullName,
                Description = qrPayment.Description,
                CreatedAt = qrPayment.CreatedAt
            };

            return new ApiResponseDto<QRPaymentDto>
            {
                Success = true,
                Message = "QR code generated successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<QRPaymentDto>
            {
                Success = false,
                Message = "Failed to generate QR code",
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponseDto<QRPaymentDto>> ScanAndPayAsync(Guid userId, ScanQRRequestDto request)
    {
        try
        {
            // Decode QR code
            var qrDataJson = Encoding.UTF8.GetString(Convert.FromBase64String(request.QrCode));
            var qrData = JsonSerializer.Deserialize<JsonElement>(qrDataJson);

            if (!qrData.TryGetProperty("id", out var qrIdElement) ||
                !qrData.TryGetProperty("userId", out var recipientIdElement))
            {
                return new ApiResponseDto<QRPaymentDto>
                {
                    Success = false,
                    Message = "Invalid QR code format"
                };
            }

            var qrId = Guid.Parse(qrIdElement.GetString()!);
            var recipientId = Guid.Parse(recipientIdElement.GetString()!);

            // Check if trying to pay yourself
            if (userId == recipientId)
            {
                return new ApiResponseDto<QRPaymentDto>
                {
                    Success = false,
                    Message = "Cannot pay to yourself"
                };
            }

            // Get the original QR payment
            var originalQrPayment = await _context.QRPayments
                .FirstOrDefaultAsync(x => x.Id == qrId && x.UserId == recipientId);

            if (originalQrPayment == null)
            {
                return new ApiResponseDto<QRPaymentDto>
                {
                    Success = false,
                    Message = "QR code not found"
                };
            }

            if (originalQrPayment.Status != GatewayStatus.Pending)
            {
                return new ApiResponseDto<QRPaymentDto>
                {
                    Success = false,
                    Message = "QR code has already been used or expired"
                };
            }

            // Validate amount
            if (request.Amount != originalQrPayment.Amount)
            {
                return new ApiResponseDto<QRPaymentDto>
                {
                    Success = false,
                    Message = "Amount mismatch with QR code"
                };
            }

            // Get recipient user to get their email
            var recipient = await _context.Users.FindAsync(recipientId);

            // Process payment
            var transferRequest = new TransferRequestDto
            {
                ReceiverEmail = recipient?.Email ?? "",
                Amount = request.Amount,
                Description = $"QR Payment: {originalQrPayment.Description}"
            };

            var transferResult = await _walletService.TransferAsync(userId, transferRequest);
            
            if (!transferResult.Success)
            {
                return new ApiResponseDto<QRPaymentDto>
                {
                    Success = false,
                    Message = transferResult.Message,
                    Error = transferResult.Error
                };
            }

            // Update original QR payment status
            originalQrPayment.Status = GatewayStatus.Success;
            originalQrPayment.UpdatedAt = DateTime.UtcNow;

            // Create payment record for payer
            var paymentRecord = new QRPayment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PaymentType = QRPaymentType.Pay,
                Amount = request.Amount,
                Description = originalQrPayment.Description,
                Status = GatewayStatus.Success,
                QrCode = request.QrCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.QRPayments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            // Send notifications
            var payerUser = await _context.Users.FindAsync(userId);
            var recipientUser = recipient; // already fetched above

            if (payerUser != null && recipientUser != null)
            {
                await _notificationService.CreateNotificationAsync(
                    userId, 
                    NotificationType.MoneySent,
                    "QR Payment Sent",
                    $"₹{request.Amount:N2} sent to {recipientUser.FullName} via QR payment",
                    "💳"
                );

                await _notificationService.CreateNotificationAsync(
                    recipientId,
                    NotificationType.MoneyReceived,
                    "QR Payment Received",
                    $"₹{request.Amount:N2} received from {payerUser.FullName} via QR payment",
                    "💰"
                );
            }

            var response = new QRPaymentDto
            {
                Id = paymentRecord.Id,
                PaymentType = paymentRecord.PaymentType.ToString(),
                QrCode = paymentRecord.QrCode,
                Amount = paymentRecord.Amount,
                Status = paymentRecord.Status.ToString(),
                MerchantName = recipient?.FullName,
                Description = paymentRecord.Description,
                CreatedAt = paymentRecord.CreatedAt
            };

            return new ApiResponseDto<QRPaymentDto>
            {
                Success = true,
                Message = "QR payment completed successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<QRPaymentDto>
            {
                Success = false,
                Message = "Failed to process QR payment",
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponseDto<PaginatedResultDto<QRPaymentDto>>> GetQRHistoryAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var query = _context.QRPayments
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var qrPayments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new QRPaymentDto
                {
                    Id = x.Id,
                    PaymentType = x.PaymentType.ToString(),
                    QrCode = x.PaymentType == QRPaymentType.Receive ? x.QrCode : "Hidden",
                    Amount = x.Amount,
                    Status = x.Status.ToString(),
                    MerchantName = x.PaymentType == QRPaymentType.Pay ? "QR Payment" : "Payment Request",
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var result = new PaginatedResultDto<QRPaymentDto>
            {
                Items = qrPayments,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new ApiResponseDto<PaginatedResultDto<QRPaymentDto>>
            {
                Success = true,
                Message = "QR payment history retrieved successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<PaginatedResultDto<QRPaymentDto>>
            {
                Success = false,
                Message = "Failed to retrieve QR payment history",
                Error = ex.Message
            };
        }
    }
}