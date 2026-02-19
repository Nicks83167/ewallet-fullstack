using EWalletAPI.Models.DTOs;

namespace EWalletAPI.Services.Interfaces;

public interface ITransactionService
{
    Task<ApiResponseDto<PaginatedTransactionsDto>> GetUserTransactionsAsync(
        Guid userId, int page = 1, int pageSize = 10);
}