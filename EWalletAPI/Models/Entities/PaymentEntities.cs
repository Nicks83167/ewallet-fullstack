using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.Entities;

public class BillPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public BillCategory Category { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ConsumerNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public GatewayStatus Status { get; set; } = GatewayStatus.Pending;
    public string? ReferenceCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RechargeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public RechargeType RechargeType { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? PlanName { get; set; }
    public decimal Amount { get; set; }
    public GatewayStatus Status { get; set; } = GatewayStatus.Pending;
    public string? ReferenceCode { get; set; }
    public bool IsFavourite { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ScheduledPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ScheduleFrequency Frequency { get; set; }
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Active;
    public DateTime NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? RecipientEmail { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class QRPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public QRPaymentType PaymentType { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public GatewayStatus Status { get; set; } = GatewayStatus.Pending;
    public string? MerchantName { get; set; }
    public string? Description { get; set; }
    public string? ReferenceCode { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DemoGatewayTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public GatewayStatus Status { get; set; } = GatewayStatus.Pending;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ReferenceCode { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
