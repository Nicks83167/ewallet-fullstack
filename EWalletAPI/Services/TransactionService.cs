using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(AppDbContext db, ILogger<TransactionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponseDto<PaginatedTransactionsDto>> GetUserTransactionsAsync(
        Guid userId, int page = 1, int pageSize = 10)
    {
        // Clamp pagination values
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Find the user's wallet
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive);

        if (wallet is null)
            return ApiResponseDto<PaginatedTransactionsDto>.Fail("Wallet not found.");

        var walletId = wallet.Id;

        // Base query: all transactions where user is sender or receiver
        var query = _db.Transactions
            .Include(t => t.SenderWallet!)
                .ThenInclude(sw => sw.User)
            .Include(t => t.ReceiverWallet!)
                .ThenInclude(rw => rw.User)
            .Where(t => t.SenderWalletId == walletId || t.ReceiverWalletId == walletId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = transactions.Select(t =>
        {
            var isSender = t.SenderWalletId == walletId;
            string? counterparty = isSender
                ? t.ReceiverWallet?.User?.FullName
                : t.SenderWallet?.User?.FullName;

            return new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Status = t.Status.ToString(),
                Description = t.Description,
                ReferenceCode = t.ReferenceCode,
                Direction = isSender ? "OUT" : "IN",
                CounterpartyName = counterparty,
                CreatedAt = t.CreatedAt
            };
        });

        return ApiResponseDto<PaginatedTransactionsDto>.Ok(new PaginatedTransactionsDto
        {
            Transactions = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }
}
