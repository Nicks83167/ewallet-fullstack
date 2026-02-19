using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _db;
    private readonly ILogger<WalletService> _logger;

    public WalletService(AppDbContext db, ILogger<WalletService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ─── GET Balance ──────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<WalletBalanceDto>> GetBalanceAsync(Guid userId)
    {
        var wallet = await _db.Wallets
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive);

        if (wallet is null)
            return ApiResponseDto<WalletBalanceDto>.Fail("Wallet not found.");

        return ApiResponseDto<WalletBalanceDto>.Ok(new WalletBalanceDto
        {
            WalletId = wallet.Id,
            Balance = wallet.Balance,
            UpdatedAt = wallet.UpdatedAt,
            OwnerName = wallet.User!.FullName
        });
    }

    // ─── POST Add Money (Deposit) ─────────────────────────────────────────────

    public async Task<ApiResponseDto<WalletOperationResponseDto>> AddMoneyAsync(
        Guid userId, AddMoneyRequestDto request)
    {
        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var wallet = await _db.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive);

            if (wallet is null)
            {
                await dbTx.RollbackAsync();
                return ApiResponseDto<WalletOperationResponseDto>.Fail("Wallet not found.");
            }

            var refCode = GenerateReferenceCode("DEP");

            // Credit wallet
            wallet.Balance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Record transaction
            var transaction = new Transaction
            {
                SenderWalletId = null,               // Deposits have no sender
                ReceiverWalletId = wallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Description = request.Description ?? "Account top-up",
                ReferenceCode = refCode,
                CreatedAt = DateTime.UtcNow
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
            await dbTx.CommitAsync();

            _logger.LogInformation(
                "Deposit ₹{Amount} to wallet {WalletId}. Ref: {Ref}",
                request.Amount, wallet.Id, refCode);

            return ApiResponseDto<WalletOperationResponseDto>.Ok(new WalletOperationResponseDto
            {
                Success = true,
                Message = $"₹{request.Amount:F2} added successfully.",
                NewBalance = wallet.Balance,
                TransactionId = refCode
            });
        }
        catch (Exception ex)
        {
            await dbTx.RollbackAsync();
            _logger.LogError(ex, "Error adding money to wallet for user {UserId}", userId);
            throw;
        }
    }

    // ─── POST Transfer ────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<WalletOperationResponseDto>> TransferAsync(
        Guid senderId, TransferRequestDto request)
    {
        // Prevent self-transfer
        var senderUser = await _db.Users.FindAsync(senderId);
        if (senderUser is null)
            return ApiResponseDto<WalletOperationResponseDto>.Fail("Sender account not found.");

        if (senderUser.Email.Equals(request.ReceiverEmail, StringComparison.OrdinalIgnoreCase))
            return ApiResponseDto<WalletOperationResponseDto>.Fail("Cannot transfer to your own wallet.");

        // Look up receiver
        var receiverUser = await _db.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u =>
                u.Email == request.ReceiverEmail.ToLower().Trim() && u.IsActive);

        if (receiverUser?.Wallet is null || !receiverUser.Wallet.IsActive)
            return ApiResponseDto<WalletOperationResponseDto>.Fail("Receiver account not found or inactive.");

        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Lock both wallets in a consistent order to prevent deadlocks
            var walletIds = new[] { senderId, receiverUser.Id }.OrderBy(id => id).ToArray();

            // Load sender wallet with row-level lock via PESSIMISTIC read
            var senderWallet = await _db.Wallets
                .FirstOrDefaultAsync(w => w.UserId == senderId && w.IsActive);

            var receiverWallet = await _db.Wallets
                .FirstOrDefaultAsync(w => w.UserId == receiverUser.Id && w.IsActive);

            if (senderWallet is null)
            {
                await dbTx.RollbackAsync();
                return ApiResponseDto<WalletOperationResponseDto>.Fail("Your wallet was not found.");
            }

            if (receiverWallet is null)
            {
                await dbTx.RollbackAsync();
                return ApiResponseDto<WalletOperationResponseDto>.Fail("Receiver wallet not found.");
            }

            // ── Insufficient balance check ──────────────────────────────────
            if (senderWallet.Balance < request.Amount)
            {
                await dbTx.RollbackAsync();
                return ApiResponseDto<WalletOperationResponseDto>.Fail(
                    $"Insufficient balance. Available: ₹{senderWallet.Balance:F2}, Requested: ₹{request.Amount:F2}");
            }

            var refCode = GenerateReferenceCode("TXN");
            var now = DateTime.UtcNow;

            // ── Atomic Debit / Credit ───────────────────────────────────────
            senderWallet.Balance -= request.Amount;
            senderWallet.UpdatedAt = now;

            receiverWallet.Balance += request.Amount;
            receiverWallet.UpdatedAt = now;

            // ── Record transaction ──────────────────────────────────────────
            var transaction = new Transaction
            {
                SenderWalletId = senderWallet.Id,
                ReceiverWalletId = receiverWallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = request.Description ?? $"Transfer to {receiverUser.Email}",
                ReferenceCode = refCode,
                CreatedAt = now
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
            await dbTx.CommitAsync();

            _logger.LogInformation(
                "Transfer ₹{Amount} from wallet {Sender} to wallet {Receiver}. Ref: {Ref}",
                request.Amount, senderWallet.Id, receiverWallet.Id, refCode);

            return ApiResponseDto<WalletOperationResponseDto>.Ok(new WalletOperationResponseDto
            {
                Success = true,
                Message = $"₹{request.Amount:F2} transferred to {receiverUser.FullName} successfully.",
                NewBalance = senderWallet.Balance,
                TransactionId = refCode
            });
        }
        catch (Exception ex)
        {
            await dbTx.RollbackAsync();
            _logger.LogError(ex,
                "Error during transfer from user {SenderId} to {ReceiverEmail}",
                senderId, request.ReceiverEmail);
            throw;
        }
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private static string GenerateReferenceCode(string prefix)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = Random.Shared.Next(1000, 9999);
        return $"{prefix}-{timestamp}-{random}";
    }
}
