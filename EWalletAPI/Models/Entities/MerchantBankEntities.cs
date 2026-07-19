namespace EWalletAPI.Models.Entities;

public class MerchantCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class Merchant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Status { get; set; } = "Active";
    public int TransactionCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public MerchantCategory? Category { get; set; }
    public ICollection<MerchantPayment> Payments { get; set; } = new List<MerchantPayment>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public class MerchantPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Completed";
    public string? ReferenceCode { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Merchant? Merchant { get; set; }
}

public class DemoBank
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Branch { get; set; }
    public int AccountCount { get; set; }
    public string Status { get; set; } = "Active";
    public decimal TotalRevenue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DemoBankAccount> Accounts { get; set; } = new List<DemoBankAccount>();
}

public class DemoBankAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BankId { get; set; }
    public Guid? UserId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DemoBank? Bank { get; set; }
}
