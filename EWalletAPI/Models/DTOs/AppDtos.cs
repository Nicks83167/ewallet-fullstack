using System.ComponentModel.DataAnnotations;
using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.DTOs;

// ─── Auth DTOs ─────────────────────────────────────────────────────────────

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        ErrorMessage = "Password must have uppercase, lowercase, digit, and special character.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public UserProfileDto User { get; set; } = null!;
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ─── Wallet DTOs ────────────────────────────────────────────────────────────

public class WalletBalanceDto
{
    public Guid WalletId { get; set; }
    public decimal Balance { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}

public class AddMoneyRequestDto
{
    [Required]
    [Range(1, 100000, ErrorMessage = "Amount must be between 1 and 100,000.")]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }
}

public class TransferRequestDto
{
    [Required(ErrorMessage = "Receiver email is required.")]
    [EmailAddress]
    public string ReceiverEmail { get; set; } = string.Empty;

    [Required]
    [Range(1, 100000, ErrorMessage = "Amount must be between 1 and 100,000.")]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }
}

public class WalletOperationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal NewBalance { get; set; }
    public string? TransactionId { get; set; }
}

// ─── Transaction DTOs ───────────────────────────────────────────────────────

public class TransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ReferenceCode { get; set; }
    public string Direction { get; set; } = string.Empty; // "Sent" or "Received"
    public string? CounterpartyName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedTransactionsDto
{
    public IEnumerable<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// ─── Common DTOs ────────────────────────────────────────────────────────────

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponseDto<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponseDto<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
