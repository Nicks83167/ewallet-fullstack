using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Icon { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

public class LinkedAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AccountType AccountType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MaskedNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

public class PaymentMethod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public PaymentMethodType MethodType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MaskedDetails { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

public class Beneficiary
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? UpiId { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public bool IsFavourite { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
