using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public Guid? SenderWalletId { get; set; }
    public Guid? ReceiverWalletId { get; set; }
    public decimal Amount { get; set; }
    public decimal? OriginalAmount { get; set; }
    public decimal? ConvertedAmount { get; set; }
    public decimal? ExchangeRate { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string? TransactionCurrency { get; set; }
    public TransactionType Type { get; set; }
    public TransactionDirection Direction { get; set; } = TransactionDirection.Out;
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? Description { get; set; }
    public string? ReferenceCode { get; set; }
    public string? Category { get; set; }
    public string? PaymentMethod { get; set; }
    public string? MerchantName { get; set; }
    public Guid? MerchantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Wallet? SenderWallet { get; set; }
    public Wallet? ReceiverWallet { get; set; }
    public Merchant? Merchant { get; set; }
}
