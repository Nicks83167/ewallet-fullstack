using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SenderWalletId { get; set; }
    public Guid? ReceiverWalletId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public string? Description { get; set; }
    public string? ReferenceCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Wallet? SenderWallet { get; set; }
    public Wallet? ReceiverWallet { get; set; }
}
