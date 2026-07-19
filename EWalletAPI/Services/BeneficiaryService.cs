using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class BeneficiaryService : IBeneficiaryService
{
    private readonly AppDbContext _context;

    public BeneficiaryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponseDto<IEnumerable<BeneficiaryDto>>> GetBeneficiariesAsync(Guid userId, string? search)
    {
        try
        {
            var query = _context.Beneficiaries.Where(x => x.UserId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search) || 
                                       (x.Phone != null && x.Phone.Contains(search)));
            }

            var beneficiaries = await query
                .OrderByDescending(x => x.IsFavourite)
                .ThenByDescending(x => x.LastUsedAt)
                .Select(x => new BeneficiaryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone,
                    UpiId = x.UpiId,
                    BankName = x.BankName,
                    IsFavourite = x.IsFavourite,
                    LastUsedAt = x.LastUsedAt ?? x.CreatedAt
                })
                .ToListAsync();

            return new ApiResponseDto<IEnumerable<BeneficiaryDto>>
            {
                Success = true,
                Message = "Beneficiaries retrieved successfully",
                Data = beneficiaries
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<IEnumerable<BeneficiaryDto>>
            {
                Success = false,
                Message = "Failed to retrieve beneficiaries"
            };
        }
    }

    public async Task<ApiResponseDto<BeneficiaryDto>> CreateBeneficiaryAsync(Guid userId, CreateBeneficiaryDto request)
    {
        try
        {
            var existingBeneficiary = await _context.Beneficiaries
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Email == request.Email);

            if (existingBeneficiary != null)
            {
                return new ApiResponseDto<BeneficiaryDto>
                {
                    Success = false,
                    Message = "Beneficiary with this email already exists"
                };
            }

            var beneficiary = new Beneficiary
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                UpiId = request.UpiId,
                BankName = request.BankName,
                IsFavourite = request.IsFavourite,
                CreatedAt = DateTime.UtcNow
            };

            _context.Beneficiaries.Add(beneficiary);
            await _context.SaveChangesAsync();

            var response = new BeneficiaryDto
            {
                Id = beneficiary.Id,
                Name = beneficiary.Name,
                Email = beneficiary.Email,
                Phone = beneficiary.Phone,
                UpiId = beneficiary.UpiId,
                BankName = beneficiary.BankName,
                IsFavourite = beneficiary.IsFavourite,
                LastUsedAt = beneficiary.LastUsedAt ?? beneficiary.CreatedAt
            };

            return new ApiResponseDto<BeneficiaryDto>
            {
                Success = true,
                Message = "Beneficiary added successfully",
                Data = response
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<BeneficiaryDto>
            {
                Success = false,
                Message = "Failed to add beneficiary"
            };
        }
    }

    public async Task<ApiResponseDto<BeneficiaryDto>> UpdateBeneficiaryAsync(Guid userId, Guid id, CreateBeneficiaryDto request)
    {
        try
        {
            var beneficiary = await _context.Beneficiaries
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (beneficiary == null)
            {
                return new ApiResponseDto<BeneficiaryDto>
                {
                    Success = false,
                    Message = "Beneficiary not found"
                };
            }

            if (beneficiary.Email != request.Email)
            {
                var existingBeneficiary = await _context.Beneficiaries
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Email == request.Email && x.Id != id);

                if (existingBeneficiary != null)
                {
                    return new ApiResponseDto<BeneficiaryDto>
                    {
                        Success = false,
                        Message = "Another beneficiary with this email already exists"
                    };
                }
            }

            beneficiary.Name = request.Name;
            beneficiary.Email = request.Email;
            beneficiary.Phone = request.Phone;
            beneficiary.UpiId = request.UpiId;
            beneficiary.BankName = request.BankName;
            beneficiary.IsFavourite = request.IsFavourite;

            await _context.SaveChangesAsync();

            var response = new BeneficiaryDto
            {
                Id = beneficiary.Id,
                Name = beneficiary.Name,
                Email = beneficiary.Email,
                Phone = beneficiary.Phone,
                UpiId = beneficiary.UpiId,
                BankName = beneficiary.BankName,
                IsFavourite = beneficiary.IsFavourite,
                LastUsedAt = beneficiary.LastUsedAt ?? beneficiary.CreatedAt
            };

            return new ApiResponseDto<BeneficiaryDto>
            {
                Success = true,
                Message = "Beneficiary updated successfully",
                Data = response
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<BeneficiaryDto>
            {
                Success = false,
                Message = "Failed to update beneficiary"
            };
        }
    }

    public async Task<ApiResponseDto<object>> DeleteBeneficiaryAsync(Guid userId, Guid id)
    {
        try
        {
            var beneficiary = await _context.Beneficiaries
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (beneficiary == null)
            {
                return new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Beneficiary not found"
                };
            }

            _context.Beneficiaries.Remove(beneficiary);
            await _context.SaveChangesAsync();

            return new ApiResponseDto<object>
            {
                Success = true,
                Message = "Beneficiary removed successfully"
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to remove beneficiary"
            };
        }
    }

    public async Task<ApiResponseDto<object>> ToggleFavouriteAsync(Guid userId, Guid id)
    {
        try
        {
            var beneficiary = await _context.Beneficiaries
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (beneficiary == null)
            {
                return new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Beneficiary not found"
                };
            }

            beneficiary.IsFavourite = !beneficiary.IsFavourite;
            await _context.SaveChangesAsync();

            return new ApiResponseDto<object>
            {
                Success = true,
                Message = beneficiary.IsFavourite ? "Added to favourites" : "Removed from favourites"
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to update favourite status"
            };
        }
    }
}