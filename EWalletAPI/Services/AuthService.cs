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
        // Check for duplicate email (case-insensitive)
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (emailExists)
            return ApiResponseDto<AuthResponseDto>.Fail("An account with this email already exists.");

        // Begin a DB transaction so User + Wallet are created atomically
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

            // Auto-create a wallet for the new user
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

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private AuthResponseDto BuildAuthResponse(User user, string token) => new()
    {
        Token = token,
        TokenType = "Bearer",
        ExpiresAt = _jwtTokenService.GetExpirationTime(),
        User = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        }
    };
}
