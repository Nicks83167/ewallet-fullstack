using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Enums;

namespace EWalletAPI.Services.Interfaces;

public interface IDashboardService
{
    Task<ApiResponseDto<DashboardOverviewDto>> GetOverviewAsync(Guid userId);
}

public interface IFeatureService
{
    Task<ApiResponseDto<PaginatedNotificationsDto>> GetNotificationsAsync(Guid userId, int page, int pageSize, bool unreadOnly);
    Task<ApiResponseDto<object>> MarkReadAsync(Guid userId, Guid notificationId);
    Task<ApiResponseDto<object>> MarkAllReadAsync(Guid userId);
    Task<ApiResponseDto<object>> DeleteNotificationAsync(Guid userId, Guid notificationId);
    Task<ApiResponseDto<IEnumerable<CurrencyDto>>> GetCurrenciesAsync();
    Task<ApiResponseDto<IEnumerable<ExchangeRateDto>>> GetExchangeRatesAsync();
    Task<ApiResponseDto<CurrencyConvertResponseDto>> ConvertCurrencyAsync(CurrencyConvertRequestDto request);
    Task<ApiResponseDto<WalletBalanceDto>> SwitchWalletCurrencyAsync(Guid userId, SwitchWalletCurrencyRequestDto request);
    Task<ApiResponseDto<IEnumerable<LinkedAccountDto>>> GetLinkedAccountsAsync(Guid userId);
    Task<ApiResponseDto<LinkedAccountDto>> CreateLinkedAccountAsync(Guid userId, CreateLinkedAccountDto request);
    Task<ApiResponseDto<object>> DeleteLinkedAccountAsync(Guid userId, Guid id);
    Task<ApiResponseDto<object>> SetDefaultLinkedAccountAsync(Guid userId, Guid id);
    Task<ApiResponseDto<IEnumerable<BeneficiaryDto>>> GetBeneficiariesAsync(Guid userId, string? search);
    Task<ApiResponseDto<BeneficiaryDto>> CreateBeneficiaryAsync(Guid userId, CreateBeneficiaryDto request);
    Task<ApiResponseDto<BeneficiaryDto>> UpdateBeneficiaryAsync(Guid userId, Guid id, CreateBeneficiaryDto request);
    Task<ApiResponseDto<object>> DeleteBeneficiaryAsync(Guid userId, Guid id);
    Task<ApiResponseDto<object>> ToggleFavouriteAsync(Guid userId, Guid id);
    Task<ApiResponseDto<IEnumerable<PaymentMethodDto>>> GetPaymentMethodsAsync(Guid userId);
}

public interface IPaymentModuleService
{
    Task<ApiResponseDto<PaginatedResultDto<BillPaymentDto>>> GetBillPaymentsAsync(Guid userId, int page, int pageSize);
    Task<ApiResponseDto<BillPaymentDto>> PayBillAsync(Guid userId, PayBillRequestDto request);
    Task<ApiResponseDto<PaginatedResultDto<RechargeRecordDto>>> GetRechargeHistoryAsync(Guid userId, int page, int pageSize);
    Task<ApiResponseDto<RechargeRecordDto>> RechargeAsync(Guid userId, RechargeRequestDto request);
    Task<ApiResponseDto<IEnumerable<ScheduledPaymentDto>>> GetScheduledPaymentsAsync(Guid userId);
    Task<ApiResponseDto<ScheduledPaymentDto>> CreateScheduledPaymentAsync(Guid userId, CreateScheduledPaymentDto request);
    Task<ApiResponseDto<object>> UpdateScheduleStatusAsync(Guid userId, Guid id, string action);
    Task<ApiResponseDto<QRPaymentDto>> GenerateQRAsync(Guid userId, GenerateQRRequestDto request);
    Task<ApiResponseDto<QRPaymentDto>> ScanAndPayAsync(Guid userId, ScanQRRequestDto request);
    Task<ApiResponseDto<PaginatedResultDto<QRPaymentDto>>> GetQRHistoryAsync(Guid userId, int page, int pageSize);
    Task<ApiResponseDto<GatewayPaymentResponseDto>> ProcessGatewayPaymentAsync(Guid userId, GatewayPaymentRequestDto request);
    Task<ApiResponseDto<IEnumerable<MerchantDto>>> GetMerchantsAsync();
    Task<ApiResponseDto<GatewayPaymentResponseDto>> PayMerchantAsync(Guid userId, Guid merchantId, decimal amount, string? description);
    Task<ApiResponseDto<IEnumerable<DemoBankDto>>> GetDemoBanksAsync();
}

public interface ISecurityService
{
    Task<ApiResponseDto<SecurityOverviewDto>> GetSecurityOverviewAsync(Guid userId);
    Task<ApiResponseDto<ExtendedProfileDto>> GetExtendedProfileAsync(Guid userId);
    Task<ApiResponseDto<ExtendedProfileDto>> UpdateExtendedProfileAsync(Guid userId, UpdateExtendedProfileDto request);
    Task<ApiResponseDto<object>> TrustDeviceAsync(Guid userId, Guid deviceId);
    Task<ApiResponseDto<object>> RevokeSessionAsync(Guid userId, Guid sessionId);
    Task RecordLoginAsync(Guid userId, string? ip, string? userAgent);
}

public interface IReportService
{
    Task<ApiResponseDto<ReportOverviewDto>> GetReportAsync(Guid userId, string period);
}

public interface ISupportService
{
    Task<ApiResponseDto<IEnumerable<SupportTicketDto>>> GetUserTicketsAsync(Guid userId);
    Task<ApiResponseDto<SupportTicketDto>> CreateTicketAsync(Guid userId, CreateTicketDto request);
    Task<ApiResponseDto<SupportReplyDto>> ReplyToTicketAsync(Guid userId, Guid ticketId, string message, bool isAdmin);
}

public interface IAdminService
{
    Task<ApiResponseDto<AdminDashboardDto>> GetDashboardAsync();
    Task<ApiResponseDto<PaginatedResultDto<AdminUserDto>>> GetUsersAsync(int page, int pageSize, string? search, string? sortBy, string? filter);
    Task<ApiResponseDto<object>> BlockUserAsync(Guid adminId, Guid userId, AdminActionDto request);
    Task<ApiResponseDto<object>> UnblockUserAsync(Guid adminId, Guid userId);
    Task<ApiResponseDto<object>> FreezeWalletAsync(Guid adminId, Guid userId);
    Task<ApiResponseDto<object>> UnfreezeWalletAsync(Guid adminId, Guid userId);
    Task<ApiResponseDto<PaginatedTransactionsDto>> GetAllTransactionsAsync(int page, int pageSize, string? status, string? currency, DateTime? from, DateTime? to);
    Task<ApiResponseDto<object>> UpdateTransactionStatusAsync(Guid adminId, Guid txId, string status);
    Task<ApiResponseDto<IEnumerable<FraudAlertDto>>> GetFraudAlertsAsync(string? status);
    Task<ApiResponseDto<object>> ResolveFraudAlertAsync(Guid adminId, Guid id, string? notes);
    Task<ApiResponseDto<PaginatedResultDto<AuditLogDto>>> GetAuditLogsAsync(int page, int pageSize);
    Task<ApiResponseDto<IEnumerable<SystemSettingDto>>> GetSystemSettingsAsync();
    Task<ApiResponseDto<object>> UpdateSystemSettingAsync(Guid adminId, SystemSettingDto request);
    Task<ApiResponseDto<object>> UpdateExchangeRateAsync(Guid adminId, UpdateExchangeRateDto request);
    Task<ApiResponseDto<object>> BroadcastNotificationAsync(Guid adminId, string title, string message, string type);
    Task<ApiResponseDto<PaginatedResultDto<SupportTicketDto>>> GetAllTicketsAsync(int page, int pageSize, string? status);
    Task<ApiResponseDto<AdminDashboardDto>> GetAnalyticsAsync(string period);
}


public interface INotificationService
{
    Task<ApiResponseDto<PaginatedNotificationsDto>> GetNotificationsAsync(Guid userId, int page, int pageSize, bool unreadOnly);
    Task<ApiResponseDto<object>> MarkReadAsync(Guid userId, Guid notificationId);
    Task<ApiResponseDto<object>> MarkAllReadAsync(Guid userId);
    Task<ApiResponseDto<object>> DeleteNotificationAsync(Guid userId, Guid notificationId);
    Task CreateNotificationAsync(Guid userId, NotificationType type, string title, string message, string? icon = null);
}

public interface ILinkedAccountService
{
    Task<ApiResponseDto<IEnumerable<LinkedAccountDto>>> GetLinkedAccountsAsync(Guid userId);
    Task<ApiResponseDto<LinkedAccountDto>> CreateLinkedAccountAsync(Guid userId, CreateLinkedAccountDto request);
    Task<ApiResponseDto<object>> DeleteLinkedAccountAsync(Guid userId, Guid id);
    Task<ApiResponseDto<object>> SetDefaultLinkedAccountAsync(Guid userId, Guid id);
}

public interface IBeneficiaryService
{
    Task<ApiResponseDto<IEnumerable<BeneficiaryDto>>> GetBeneficiariesAsync(Guid userId, string? search);
    Task<ApiResponseDto<BeneficiaryDto>> CreateBeneficiaryAsync(Guid userId, CreateBeneficiaryDto request);
    Task<ApiResponseDto<BeneficiaryDto>> UpdateBeneficiaryAsync(Guid userId, Guid id, CreateBeneficiaryDto request);
    Task<ApiResponseDto<object>> DeleteBeneficiaryAsync(Guid userId, Guid id);
    Task<ApiResponseDto<object>> ToggleFavouriteAsync(Guid userId, Guid id);
}

public interface ICurrencyService
{
    Task<ApiResponseDto<IEnumerable<CurrencyDto>>> GetCurrenciesAsync();
    Task<ApiResponseDto<IEnumerable<ExchangeRateDto>>> GetExchangeRatesAsync();
    Task<ApiResponseDto<CurrencyConvertResponseDto>> ConvertCurrencyAsync(CurrencyConvertRequestDto request);
    Task<ApiResponseDto<WalletBalanceDto>> SwitchWalletCurrencyAsync(Guid userId, SwitchWalletCurrencyRequestDto request);
}

public interface IQRPaymentService
{
    Task<ApiResponseDto<QRPaymentDto>> GenerateQRAsync(Guid userId, GenerateQRRequestDto request);
    Task<ApiResponseDto<QRPaymentDto>> ScanAndPayAsync(Guid userId, ScanQRRequestDto request);
    Task<ApiResponseDto<PaginatedResultDto<QRPaymentDto>>> GetQRHistoryAsync(Guid userId, int page, int pageSize);
}

public interface IBillPaymentService
{
    Task<ApiResponseDto<PaginatedResultDto<BillPaymentDto>>> GetBillPaymentsAsync(Guid userId, int page, int pageSize);
    Task<ApiResponseDto<BillPaymentDto>> PayBillAsync(Guid userId, PayBillRequestDto request);
}

public interface IRechargeService
{
    Task<ApiResponseDto<PaginatedResultDto<RechargeRecordDto>>> GetRechargeHistoryAsync(Guid userId, int page, int pageSize);
    Task<ApiResponseDto<RechargeRecordDto>> RechargeAsync(Guid userId, RechargeRequestDto request);
}

public interface IScheduledPaymentService
{
    Task<ApiResponseDto<IEnumerable<ScheduledPaymentDto>>> GetScheduledPaymentsAsync(Guid userId);
    Task<ApiResponseDto<ScheduledPaymentDto>> CreateScheduledPaymentAsync(Guid userId, CreateScheduledPaymentDto request);
    Task<ApiResponseDto<object>> UpdateScheduleStatusAsync(Guid userId, Guid id, string action);
}
