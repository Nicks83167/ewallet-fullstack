using EWalletAPI.Models.DTOs;

namespace EWalletAPI.Services.Interfaces;

public interface IWalletService
{
    Task<ApiResponseDto<WalletBalanceDto>> GetBalanceAsync(Guid userId);
    Task<ApiResponseDto<WalletOperationResponseDto>> AddMoneyAsync(Guid userId, AddMoneyRequestDto request);
    Task<ApiResponseDto<WalletOperationResponseDto>> TransferAsync(Guid senderId, TransferRequestDto request);
    Task<ApiResponseDto<WalletOperationResponseDto>> WithdrawAsync(Guid userId, WithdrawRequestDto request);
}
