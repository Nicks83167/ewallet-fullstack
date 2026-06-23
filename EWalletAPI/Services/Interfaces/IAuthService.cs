using EWalletAPI.Models.DTOs;

namespace EWalletAPI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponseDto<UserProfileDto>> GetProfileAsync(Guid userId);
    Task<ApiResponseDto<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
    Task<ApiResponseDto<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
}
