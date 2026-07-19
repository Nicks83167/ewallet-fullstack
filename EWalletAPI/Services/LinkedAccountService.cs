using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class LinkedAccountService : ILinkedAccountService
{
    private readonly AppDbContext _context;

    public LinkedAccountService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<IEnumerable<LinkedAccountDto>>> GetLinkedAccountsAsync(Guid userId)
    {
        try
        {
            var accounts = await _context.LinkedAccounts
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.CreatedAt)
                .Select(x => new LinkedAccountDto
                {
                    Id = x.Id,
                    AccountType = x.AccountType.ToString(),
                    Name = x.Name,
                    MaskedNumber = x.MaskedNumber,
                    BankName = x.BankName,
                    LogoUrl = x.LogoUrl,
                    IsDefault = x.IsDefault,
                    IsVerified = x.IsVerified,
                    Status = x.Status
                })
                .ToListAsync();

            return new ApiResponseDto<IEnumerable<LinkedAccountDto>>
            {
                Success = true,
                Message = "Linked accounts retrieved successfully",
                Data = accounts
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<IEnumerable<LinkedAccountDto>>
            {
                Success = false,
                Message = "Failed to retrieve linked accounts"
            };
        }
    }

    public async Task<ApiResponseDto<LinkedAccountDto>> CreateLinkedAccountAsync(Guid userId, CreateLinkedAccountDto request)
    {
        try
        {
            if (!Enum.TryParse<AccountType>(request.AccountType, out var accountType))
            {
                return new ApiResponseDto<LinkedAccountDto>
                {
                    Success = false,
                    Message = "Invalid account type"
                };
            }

            var maskedNumber = MaskAccountNumber(request.AccountNumber);

            var account = new LinkedAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountType = accountType,
                Name = request.Name,
                MaskedNumber = maskedNumber,
                BankName = request.BankName,
                IsDefault = false,
                IsVerified = accountType == AccountType.UpiId,
                CreatedAt = DateTime.UtcNow
            };

            var hasAccounts = await _context.LinkedAccounts.AnyAsync(x => x.UserId == userId);
            if (!hasAccounts)
            {
                account.IsDefault = true;
            }

            _context.LinkedAccounts.Add(account);
            await _context.SaveChangesAsync();

            var response = new LinkedAccountDto
            {
                Id = account.Id,
                AccountType = account.AccountType.ToString(),
                Name = account.Name,
                MaskedNumber = account.MaskedNumber,
                BankName = account.BankName,
                LogoUrl = account.LogoUrl,
                IsDefault = account.IsDefault,
                IsVerified = account.IsVerified,
                Status = account.Status
            };

            return new ApiResponseDto<LinkedAccountDto>
            {
                Success = true,
                Message = "Account linked successfully",
                Data = response
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<LinkedAccountDto>
            {
                Success = false,
                Message = "Failed to link account"
            };
        }
    }

    public async Task<ApiResponseDto<object>> DeleteLinkedAccountAsync(Guid userId, Guid id)
    {
        try
        {
            var account = await _context.LinkedAccounts
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (account == null)
            {
                return new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Account not found"
                };
            }

            if (account.IsDefault)
            {
                var otherAccounts = await _context.LinkedAccounts
                    .Where(x => x.UserId == userId && x.Id != id)
                    .ToListAsync();

                if (otherAccounts.Any())
                {
                    otherAccounts.First().IsDefault = true;
                }
            }

            _context.LinkedAccounts.Remove(account);
            await _context.SaveChangesAsync();

            return new ApiResponseDto<object>
            {
                Success = true,
                Message = "Account removed successfully"
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to remove account"
            };
        }
    }

    public async Task<ApiResponseDto<object>> SetDefaultLinkedAccountAsync(Guid userId, Guid id)
    {
        try
        {
            var account = await _context.LinkedAccounts
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (account == null)
            {
                return new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Account not found"
                };
            }

            var otherAccounts = await _context.LinkedAccounts
                .Where(x => x.UserId == userId && x.Id != id)
                .ToListAsync();

            foreach (var otherAccount in otherAccounts)
            {
                otherAccount.IsDefault = false;
            }

            account.IsDefault = true;
            await _context.SaveChangesAsync();

            return new ApiResponseDto<object>
            {
                Success = true,
                Message = "Default account updated successfully"
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to update default account"
            };
        }
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
            return "****";

        return "****" + accountNumber[^4..];
    }
}