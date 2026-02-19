using EWalletAPI.Models.Entities;

namespace EWalletAPI.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    DateTime GetExpirationTime();
}