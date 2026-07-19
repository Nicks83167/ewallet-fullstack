using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }
    public bool IsBlocked { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? PanNumber { get; set; }
    public string? AadhaarNumber { get; set; }
    public int ProfileCompletionScore { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public Wallet? Wallet { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<LinkedAccount> LinkedAccounts { get; set; } = new List<LinkedAccount>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
