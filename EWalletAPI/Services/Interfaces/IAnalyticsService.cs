using EWalletAPI.Models.DTOs;

namespace EWalletAPI.Services.Interfaces;

public interface IAnalyticsService
{
    Task<ApiResponseDto<DashboardAnalyticsDto>> GetDashboardAnalyticsAsync(Guid userId);
    Task<ApiResponseDto<ReportOverviewDto>> GenerateReportAsync(Guid userId, string period, DateTime? startDate, DateTime? endDate);
}
