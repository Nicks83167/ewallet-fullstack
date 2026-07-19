using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.Entities;

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0.00m;
    public string CurrencyCode { get; set; } = "INR";
    public WalletStatus Status { get; set; } = WalletStatus.Active;
    public decimal DailyLimit { get; set; } = 100000m;
    public decimal MonthlyLimit { get; set; } = 500000m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
}
