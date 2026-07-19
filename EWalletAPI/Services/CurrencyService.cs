using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Entities;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class CurrencyService : ICurrencyService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;

    public CurrencyService(AppDbContext context, IWalletService walletService)
    {
        _context = context;
        _walletService = walletService;
    }

    public async Task<ApiResponseDto<IEnumerable<CurrencyDto>>> GetCurrenciesAsync()
    {
        try
        {
            var currencies = await _context.Currencies
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new CurrencyDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Symbol = x.Symbol
                })
                .ToListAsync();

            // Add some demo currencies if none exist
            if (!currencies.Any())
            {
                currencies = GetDemoCurrencies().ToList();
            }

            return new ApiResponseDto<IEnumerable<CurrencyDto>>
            {
                Success = true,
                Message = "Currencies retrieved successfully",
                Data = currencies
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<IEnumerable<CurrencyDto>>
            {
                Success = false,
                Message = "Failed to retrieve currencies"
            };
        }
    }

    public async Task<ApiResponseDto<IEnumerable<ExchangeRateDto>>> GetExchangeRatesAsync()
    {
        try
        {
            var exchangeRates = await _context.ExchangeRates
                .OrderBy(x => x.FromCurrency)
                .ThenBy(x => x.ToCurrency)
                .Select(x => new ExchangeRateDto
                {
                    FromCurrency = x.FromCurrency,
                    ToCurrency = x.ToCurrency,
                    Rate = x.Rate,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            // Add some demo exchange rates if none exist
            if (!exchangeRates.Any())
            {
                exchangeRates = GetDemoExchangeRates().ToList();
            }

            return new ApiResponseDto<IEnumerable<ExchangeRateDto>>
            {
                Success = true,
                Message = "Exchange rates retrieved successfully",
                Data = exchangeRates
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<IEnumerable<ExchangeRateDto>>
            {
                Success = false,
                Message = "Failed to retrieve exchange rates"
            };
        }
    }

    public async Task<ApiResponseDto<CurrencyConvertResponseDto>> ConvertCurrencyAsync(CurrencyConvertRequestDto request)
    {
        try
        {
            var rate = await GetExchangeRateAsync(request.FromCurrency, request.ToCurrency);
            
            if (rate == 0)
            {
                return new ApiResponseDto<CurrencyConvertResponseDto>
                {
                    Success = false,
                    Message = $"Exchange rate not found for {request.FromCurrency} to {request.ToCurrency}"
                };
            }

            var convertedAmount = request.Amount * rate;

            var response = new CurrencyConvertResponseDto
            {
                OriginalAmount = request.Amount,
                ConvertedAmount = Math.Round(convertedAmount, 2),
                ExchangeRate = rate,
                FromCurrency = request.FromCurrency,
                ToCurrency = request.ToCurrency,
                FormattedOriginal = FormatCurrency(request.Amount, request.FromCurrency),
                FormattedConverted = FormatCurrency(convertedAmount, request.ToCurrency)
            };

            return new ApiResponseDto<CurrencyConvertResponseDto>
            {
                Success = true,
                Message = "Currency converted successfully",
                Data = response
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<CurrencyConvertResponseDto>
            {
                Success = false,
                Message = "Failed to convert currency"
            };
        }
    }

    public async Task<ApiResponseDto<WalletBalanceDto>> SwitchWalletCurrencyAsync(Guid userId, SwitchWalletCurrencyRequestDto request)
    {
        try
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (wallet == null)
            {
                return new ApiResponseDto<WalletBalanceDto>
                {
                    Success = false,
                    Message = "Wallet not found"
                };
            }

            // Check if currency is valid
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(x => x.Code == request.CurrencyCode && x.IsActive);

            if (currency == null && !IsDemoCurrency(request.CurrencyCode))
            {
                return new ApiResponseDto<WalletBalanceDto>
                {
                    Success = false,
                    Message = "Invalid currency code"
                };
            }

            // Convert current balance to new currency
            if (wallet.CurrencyCode != request.CurrencyCode)
            {
                var rate = await GetExchangeRateAsync(wallet.CurrencyCode, request.CurrencyCode);
                if (rate > 0)
                {
                    wallet.Balance = Math.Round(wallet.Balance * rate, 2);
                }
                wallet.CurrencyCode = request.CurrencyCode;
                wallet.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var response = new WalletBalanceDto
            {
                Balance = wallet.Balance,
                CurrencyCode = wallet.CurrencyCode,
                CurrencySymbol = GetCurrencySymbol(wallet.CurrencyCode)
            };

            return new ApiResponseDto<WalletBalanceDto>
            {
                Success = true,
                Message = "Wallet currency updated successfully",
                Data = response
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<WalletBalanceDto>
            {
                Success = false,
                Message = "Failed to switch wallet currency"
            };
        }
    }

    private async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency)
            return 1.0m;

        var exchangeRate = await _context.ExchangeRates
            .FirstOrDefaultAsync(x => x.FromCurrency == fromCurrency && x.ToCurrency == toCurrency);

        if (exchangeRate != null)
            return exchangeRate.Rate;

        // Try reverse rate
        var reverseRate = await _context.ExchangeRates
            .FirstOrDefaultAsync(x => x.FromCurrency == toCurrency && x.ToCurrency == fromCurrency);

        if (reverseRate != null)
            return 1.0m / reverseRate.Rate;

        // Return demo rates for common pairs
        return GetDemoExchangeRate(fromCurrency, toCurrency);
    }

    private static decimal GetDemoExchangeRate(string fromCurrency, string toCurrency)
    {
        var pair = $"{fromCurrency}{toCurrency}";
        return pair switch
        {
            "INRUSD" => 0.012m,
            "USDINR" => 83.25m,
            "INREUR" => 0.011m,
            "EURINR" => 90.15m,
            "INRGBP" => 0.0095m,
            "GBPINR" => 105.20m,
            "USDEUR" => 0.92m,
            "EURUSD" => 1.09m,
            "USDGBP" => 0.79m,
            "GBPUSD" => 1.27m,
            _ => 1.0m
        };
    }

    private static IEnumerable<CurrencyDto> GetDemoCurrencies()
    {
        return new[]
        {
            new CurrencyDto { Id = Guid.NewGuid(), Code = "INR", Name = "Indian Rupee", Symbol = "₹" },
            new CurrencyDto { Id = Guid.NewGuid(), Code = "USD", Name = "US Dollar", Symbol = "$" },
            new CurrencyDto { Id = Guid.NewGuid(), Code = "EUR", Name = "Euro", Symbol = "€" },
            new CurrencyDto { Id = Guid.NewGuid(), Code = "GBP", Name = "British Pound", Symbol = "£" },
            new CurrencyDto { Id = Guid.NewGuid(), Code = "JPY", Name = "Japanese Yen", Symbol = "¥" },
            new CurrencyDto { Id = Guid.NewGuid(), Code = "AUD", Name = "Australian Dollar", Symbol = "A$" },
            new CurrencyDto { Id = Guid.NewGuid(), Code = "CAD", Name = "Canadian Dollar", Symbol = "C$" },
            new CurrencyDto { Id = Guid.NewGuid(), Code = "CHF", Name = "Swiss Franc", Symbol = "Fr" }
        };
    }

    private static IEnumerable<ExchangeRateDto> GetDemoExchangeRates()
    {
        return new[]
        {
            new ExchangeRateDto { FromCurrency = "INR", ToCurrency = "USD", Rate = 0.012m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "USD", ToCurrency = "INR", Rate = 83.25m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "INR", ToCurrency = "EUR", Rate = 0.011m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "EUR", ToCurrency = "INR", Rate = 90.15m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "INR", ToCurrency = "GBP", Rate = 0.0095m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "GBP", ToCurrency = "INR", Rate = 105.20m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "USD", ToCurrency = "EUR", Rate = 0.92m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "EUR", ToCurrency = "USD", Rate = 1.09m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "USD", ToCurrency = "GBP", Rate = 0.79m, UpdatedAt = DateTime.UtcNow },
            new ExchangeRateDto { FromCurrency = "GBP", ToCurrency = "USD", Rate = 1.27m, UpdatedAt = DateTime.UtcNow }
        };
    }

    private static bool IsDemoCurrency(string currencyCode)
    {
        var demoCurrencies = new[] { "INR", "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF" };
        return demoCurrencies.Contains(currencyCode);
    }

    private static string GetCurrencySymbol(string currencyCode)
    {
        return currencyCode switch
        {
            "INR" => "₹",
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "JPY" => "¥",
            "AUD" => "A$",
            "CAD" => "C$",
            "CHF" => "Fr",
            _ => currencyCode
        };
    }

    private static string FormatCurrency(decimal amount, string currencyCode)
    {
        var symbol = GetCurrencySymbol(currencyCode);
        return $"{symbol}{amount:N2}";
    }
}