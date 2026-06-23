using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (emailExists)
            return ApiResponseDto<AuthResponseDto>.Fail("An account with this email already exists.");

        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var wallet = new Wallet
            {
                UserId = user.Id,
                Balance = 0.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync();

            await dbTx.CommitAsync();

            _logger.LogInformation("New user registered: {Email}", user.Email);

            var token = _jwtTokenService.GenerateToken(user);
            return ApiResponseDto<AuthResponseDto>.Ok(BuildAuthResponse(user, token), "Registration successful.");
        }
        catch (Exception ex)
        {
            await dbTx.RollbackAsync();
            _logger.LogError(ex, "Error during user registration for {Email}", request.Email);
            throw;
        }
    }

    public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

        if (user is null || !user.IsActive)
            return ApiResponseDto<AuthResponseDto>.Fail("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return ApiResponseDto<AuthResponseDto>.Fail("Invalid email or password.");
        }

        _logger.LogInformation("User logged in: {Email}", user.Email);

        var token = _jwtTokenService.GenerateToken(user);
        return ApiResponseDto<AuthResponseDto>.Ok(BuildAuthResponse(user, token), "Login successful.");
    }

    public async Task<ApiResponseDto<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsActive)
            return ApiResponseDto<UserProfileDto>.Fail("User not found.");

        return ApiResponseDto<UserProfileDto>.Ok(MapToProfileDto(user));
    }

    public async Task<ApiResponseDto<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsActive)
            return ApiResponseDto<UserProfileDto>.Fail("User not found.");

        user.FullName = request.FullName.Trim();
        await _db.SaveChangesAsync();

        _logger.LogInformation("Profile updated for user {UserId}", userId);
        return ApiResponseDto<UserProfileDto>.Ok(MapToProfileDto(user), "Profile updated successfully.");
    }

    public async Task<ApiResponseDto<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsActive)
            return ApiResponseDto<object>.Fail("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ApiResponseDto<object>.Fail("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return ApiResponseDto<object>.Ok(new { }, "Password changed successfully.");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private AuthResponseDto BuildAuthResponse(User user, string token) => new()
    {
        Token = token,
        TokenType = "Bearer",
        ExpiresAt = _jwtTokenService.GetExpirationTime(),
        User = MapToProfileDto(user)
    };

    private static UserProfileDto MapToProfileDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        CreatedAt = user.CreatedAt
    };
}
