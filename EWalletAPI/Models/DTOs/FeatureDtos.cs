using System.ComponentModel.DataAnnotations;
using EWalletAPI.Models.Enums;

namespace EWalletAPI.Models.DTOs;

// ─── Dashboard DTOs ───────────────────────────────────────────────────────────

public class DashboardOverviewDto
{
    public WalletBalanceDto Wallet { get; set; } = null!;
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
    public string WalletStatus { get; set; } = "Active";
    public bool IsVerified { get; set; }
    public int ProfileCompletionScore { get; set; }
    public QuickInsightsDto Insights { get; set; } = null!;
    public IEnumerable<ActivityTimelineItemDto> RecentActivity { get; set; } = [];
    public IEnumerable<ChartDataPointDto> IncomeVsExpense { get; set; } = [];
    public IEnumerable<ChartDataPointDto> WeeklySpending { get; set; } = [];
    public IEnumerable<ChartDataPointDto> MonthlySpending { get; set; } = [];
    public IEnumerable<ChartDataPointDto> CategoryDistribution { get; set; } = [];
    public IEnumerable<ChartDataPointDto> PaymentMethodDistribution { get; set; } = [];
    public IEnumerable<ChartDataPointDto> SavingsTrend { get; set; } = [];
    public IEnumerable<ChartDataPointDto> MonthlyComparison { get; set; } = [];
    public CurrentMonthStatsDto CurrentMonthStats { get; set; } = null!;
}

public class QuickInsightsDto
{
    public decimal HighestTransaction { get; set; }
    public decimal AverageTransaction { get; set; }
    public decimal AverageDailySpending { get; set; }
    public decimal AverageMonthlySpending { get; set; }
    public int WalletAgeDays { get; set; }
    public int TotalTransactions { get; set; }
    public int PendingRequests { get; set; }
    public int CompletedTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int SavedBeneficiaries { get; set; }
    public int LinkedAccounts { get; set; }
    public int UnreadNotifications { get; set; }
}

public class ActivityTimelineItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ChartDataPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? Value2 { get; set; }
    public string? Category { get; set; }
}

public class CurrentMonthStatsDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetSavings { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageDailySpending { get; set; }
}

// ─── Notification DTOs ──────────────────────────────────────────────────────

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedNotificationsDto
{
    public IEnumerable<NotificationDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// ─── Currency DTOs ────────────────────────────────────────────────────────────

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}

public class ExchangeRateDto
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CurrencyConvertRequestDto
{
    [Required] public string FromCurrency { get; set; } = "INR";
    [Required] public string ToCurrency { get; set; } = "USD";
    [Required][Range(0.01, 10000000)] public decimal Amount { get; set; }
}

public class CurrencyConvertResponseDto
{
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public string FormattedOriginal { get; set; } = string.Empty;
    public string FormattedConverted { get; set; } = string.Empty;
}

public class SwitchWalletCurrencyRequestDto
{
    [Required] public string CurrencyCode { get; set; } = "INR";
}

// ─── Linked Account DTOs ──────────────────────────────────────────────────────

public class LinkedAccountDto
{
    public Guid Id { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MaskedNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateLinkedAccountDto
{
    [Required] public string AccountType { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string AccountNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
}

// ─── Beneficiary DTOs ─────────────────────────────────────────────────────────

public class BeneficiaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? UpiId { get; set; }
    public string? BankName { get; set; }
    public bool IsFavourite { get; set; }
    public DateTime LastUsedAt { get; set; }
}

public class CreateBeneficiaryDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? UpiId { get; set; }
    public string? BankName { get; set; }
    public bool IsFavourite { get; set; }
}

// ─── Payment Method DTOs ──────────────────────────────────────────────────────

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string MethodType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? MaskedDetails { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ─── Bill Payment DTOs ────────────────────────────────────────────────────────

public class BillPaymentDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ConsumerNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PayBillRequestDto
{
    [Required] public string Category { get; set; } = string.Empty;
    [Required] public string Provider { get; set; } = string.Empty;
    [Required] public string ConsumerNumber { get; set; } = string.Empty;
    [Required][Range(1, 100000)] public decimal Amount { get; set; }
}

// ─── Recharge DTOs ────────────────────────────────────────────────────────────

public class RechargeRecordDto
{
    public Guid Id { get; set; }
    public string RechargeType { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? PlanName { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsFavourite { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RechargeRequestDto
{
    [Required] public string RechargeType { get; set; } = "Mobile";
    [Required] public string Operator { get; set; } = string.Empty;
    [Required] public string MobileNumber { get; set; } = string.Empty;
    public string? PlanName { get; set; }
    [Required][Range(1, 10000)] public decimal Amount { get; set; }
}

// ─── Scheduled Payment DTOs ───────────────────────────────────────────────────

public class ScheduledPaymentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string? RecipientEmail { get; set; }
}

public class CreateScheduledPaymentDto
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string PaymentType { get; set; } = string.Empty;
    [Required][Range(1, 100000)] public decimal Amount { get; set; }
    [Required] public string Frequency { get; set; } = "Monthly";
    public string? RecipientEmail { get; set; }
    public string? Description { get; set; }
}

// ─── QR Payment DTOs ──────────────────────────────────────────────────────────

public class QRPaymentDto
{
    public Guid Id { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GenerateQRRequestDto
{
    [Required][Range(1, 100000)] public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class ScanQRRequestDto
{
    [Required] public string QrCode { get; set; } = string.Empty;
    [Required][Range(1, 100000)] public decimal Amount { get; set; }
}

// ─── Gateway DTOs ─────────────────────────────────────────────────────────────

public class GatewayPaymentRequestDto
{
    [Required][Range(1, 100000)] public decimal Amount { get; set; }
    [Required] public string PaymentMethod { get; set; } = "WalletBalance";
    public string? Description { get; set; }
    public bool SimulateFailure { get; set; }
}

public class GatewayPaymentResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ─── Security DTOs ────────────────────────────────────────────────────────────

public class DeviceDto
{
    public Guid Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? Browser { get; set; }
    public string? Os { get; set; }
    public string? IpAddress { get; set; }
    public bool IsTrusted { get; set; }
    public DateTime LastActiveAt { get; set; }
}

public class LoginHistoryDto
{
    public Guid Id { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public string? Device { get; set; }
    public bool IsSuccessful { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SecurityOverviewDto
{
    public int SecurityScore { get; set; }
    public int ProfileCompletionScore { get; set; }
    public int TrustedDevices { get; set; }
    public int ActiveSessions { get; set; }
    public IEnumerable<LoginHistoryDto> RecentLogins { get; set; } = [];
    public IEnumerable<DeviceDto> Devices { get; set; } = [];
}

public class ExtendedProfileDto : UserProfileDto
{
    public string? Phone { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? PanNumber { get; set; }
    public string? AadhaarNumber { get; set; }
    public bool IsVerified { get; set; }
    public int ProfileCompletionScore { get; set; }
    public string WalletCurrency { get; set; } = "INR";
    public string WalletStatus { get; set; } = "Active";
}

public class UpdateExtendedProfileDto
{
    [Required] public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? PanNumber { get; set; }
    public string? AadhaarNumber { get; set; }
}

// ─── Report DTOs ──────────────────────────────────────────────────────────────

public class ReportOverviewDto
{
    public IEnumerable<ChartDataPointDto> IncomeExpense { get; set; } = [];
    public IEnumerable<ChartDataPointDto> CategoryBreakdown { get; set; } = [];
    public IEnumerable<ChartDataPointDto> CashFlow { get; set; } = [];
    public IEnumerable<ChartDataPointDto> TopMerchants { get; set; } = [];
    public IEnumerable<TransactionDto> LargestTransactions { get; set; } = [];
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalSavings { get; set; }
    public string Period { get; set; } = "Monthly";
}

// ─── Support DTOs ─────────────────────────────────────────────────────────────

public class SupportTicketDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<SupportReplyDto> Replies { get; set; } = [];
}

public class SupportReplyDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTicketDto
{
    [Required] public string Subject { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
}

// ─── Admin DTOs ───────────────────────────────────────────────────────────────

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TodayUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int BlockedUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public decimal TotalWalletBalance { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodayTransactions { get; set; }
    public int MonthlyTransactions { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }
    public int RefundRequests { get; set; }
    public int ScheduledPayments { get; set; }
    public int MerchantPayments { get; set; }
    public int QrTransactions { get; set; }
    public int FraudAlerts { get; set; }
    public int PendingReviews { get; set; }
    public IEnumerable<ChartDataPointDto> DailyTransactions { get; set; } = [];
    public IEnumerable<ChartDataPointDto> RevenueGrowth { get; set; } = [];
    public IEnumerable<ChartDataPointDto> UserGrowth { get; set; } = [];
    public IEnumerable<ChartDataPointDto> PaymentMethodDistribution { get; set; } = [];
    public IEnumerable<ChartDataPointDto> CurrencyDistribution { get; set; } = [];
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBlocked { get; set; }
    public decimal WalletBalance { get; set; }
    public string WalletStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaginatedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class MerchantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Category { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DemoBankDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Branch { get; set; }
    public int AccountCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class FraudAlertDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FraudScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SystemSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateExchangeRateDto
{
    [Required] public string FromCurrency { get; set; } = string.Empty;
    [Required] public string ToCurrency { get; set; } = string.Empty;
    [Required][Range(0.000001, 1000000)] public decimal Rate { get; set; }
}

public class AdminActionDto
{
    public string Action { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
