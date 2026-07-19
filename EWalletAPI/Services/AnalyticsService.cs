using EWalletAPI.Data;
using EWalletAPI.Models.DTOs;
using EWalletAPI.Models.Enums;
using EWalletAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(AppDbContext db, ILogger<AnalyticsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponseDto<DashboardAnalyticsDto>> GetDashboardAnalyticsAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.Wallet)
            .Include(u => u.LinkedAccounts)
            .Include(u => u.Beneficiaries)
            .Include(u => u.Notifications)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user?.Wallet is null)
            return ApiResponseDto<DashboardAnalyticsDto>.Fail("User or wallet not found.");

        var wallet = user.Wallet;
        var walletId = wallet.Id;

        // Get transactions for current month
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var allTransactions = await _db.Transactions
            .Where(t => (t.SenderWalletId == walletId || t.ReceiverWalletId == walletId) 
                && t.Status == TransactionStatus.Completed)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var monthTransactions = allTransactions
            .Where(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd)
            .ToList();

        var inTransactions = monthTransactions.Where(t => t.ReceiverWalletId == walletId).ToList();
        var outTransactions = monthTransactions.Where(t => t.SenderWalletId == walletId).ToList();

        var totalIncome = inTransactions.Sum(t => t.Amount);
        var totalExpense = outTransactions.Sum(t => t.Amount);

        // Recent activities (last 10)
        var recentActivities = await BuildRecentActivitiesAsync(walletId, allTransactions.Take(10).ToList());

        // Quick insights
        var completedTransactions = allTransactions.Where(t => t.Status == TransactionStatus.Completed).ToList();
        var allAmounts = completedTransactions.Select(t => t.Amount).ToList();

        var insights = new QuickInsightsDto
        {
            HighestTransaction = allAmounts.Any() ? allAmounts.Max() : 0,
            AverageTransaction = allAmounts.Any() ? allAmounts.Average() : 0,
            AverageDailySpending = totalExpense > 0 ? totalExpense / DateTime.DaysInMonth(now.Year, now.Month) : 0,
            AverageMonthlySpending = totalExpense,
            WalletAgeDays = (int)(now - wallet.CreatedAt).TotalDays,
            TotalTransactions = completedTransactions.Count,
            PendingRequests = allTransactions.Count(t => t.Status == TransactionStatus.Pending),
            CompletedTransactions = completedTransactions.Count,
            FailedTransactions = allTransactions.Count(t => t.Status == TransactionStatus.Failed),
            SavedBeneficiaries = user.Beneficiaries.Count,
            LinkedAccounts = user.LinkedAccounts.Count
        };

        // Weekly spending (last 4 weeks)
        var weeklySpending = new List<ChartDataPoint>();
        for (int i = 3; i >= 0; i--)
        {
            var weekStart = now.AddDays(-7 * i).AddDays(-7);
            var weekEnd = weekStart.AddDays(7);
            var weekExpense = allTransactions
                .Where(t => t.SenderWalletId == walletId && t.CreatedAt >= weekStart && t.CreatedAt < weekEnd)
                .Sum(t => t.Amount);
            weeklySpending.Add(new ChartDataPoint
            {
                Label = $"Week {4 - i}",
                Value = weekExpense
            });
        }

        // Category distribution
        var categoryDistribution = outTransactions
            .GroupBy(t => t.Category ?? "Other")
            .Select(g => new ChartDataPoint
            {
                Label = g.Key,
                Value = g.Sum(t => t.Amount)
            })
            .OrderByDescending(c => c.Value)
            .Take(5)
            .ToList();

        // Income vs Expense (last 6 months)
        var incomeVsExpense = new List<ChartDataPoint>();
        for (int i = 5; i >= 0; i--)
        {
            var month = now.AddMonths(-i);
            var monthStartDate = new DateTime(month.Year, month.Month, 1);
            var monthEndDate = monthStartDate.AddMonths(1);

            var monthIn = allTransactions
                .Where(t => t.ReceiverWalletId == walletId && t.CreatedAt >= monthStartDate && t.CreatedAt < monthEndDate)
                .Sum(t => t.Amount);
            var monthOut = allTransactions
                .Where(t => t.SenderWalletId == walletId && t.CreatedAt >= monthStartDate && t.CreatedAt < monthEndDate)
                .Sum(t => t.Amount);

            incomeVsExpense.Add(new ChartDataPoint
            {
                Label = monthStartDate.ToString("MMM"),
                Value = monthIn,
                Color = "#10b981"
            });
            incomeVsExpense.Add(new ChartDataPoint
            {
                Label = monthStartDate.ToString("MMM"),
                Value = monthOut,
                Color = "#ef4444"
            });
        }

        // Top transactions
        var topTransactions = completedTransactions
            .OrderByDescending(t => t.Amount)
            .Take(5)
            .Select(t => new TopTransactionDto
            {
                Id = t.Id,
                Description = t.Description ?? t.Type.ToString(),
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Date = t.CreatedAt
            })
            .ToList();

        var analytics = new DashboardAnalyticsDto
        {
            WalletOverview = new WalletOverviewDto
            {
                CurrentBalance = wallet.Balance,
                MoneyIn = totalIncome,
                MoneyOut = totalExpense,
                WalletId = wallet.Id.ToString(),
                Currency = wallet.CurrencyCode,
                WalletStatus = wallet.Status.ToString(),
                IsVerified = user.IsVerified,
                ProfileCompletionScore = user.ProfileCompletionScore
            },
            MonthlyAnalytics = new MonthlyAnalyticsDto
            {
                IncomeVsExpense = incomeVsExpense,
                WeeklySpending = weeklySpending,
                CategoryDistribution = categoryDistribution,
                LargestTransactions = topTransactions,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetSavings = totalIncome - totalExpense,
                AverageDailySpending = insights.AverageDailySpending,
                MostUsedPaymentMethod = outTransactions
                    .GroupBy(t => t.PaymentMethod ?? "WalletBalance")
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "WalletBalance"
            },
            QuickInsights = insights,
            RecentActivities = recentActivities
        };

        return ApiResponseDto<DashboardAnalyticsDto>.Ok(analytics);
    }

    public async Task<ApiResponseDto<ReportOverviewDto>> GenerateReportAsync(
        Guid userId, string period, DateTime? startDate, DateTime? endDate)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive);
        if (wallet is null)
            return ApiResponseDto<ReportOverviewDto>.Fail("Wallet not found.");

        // Determine date range
        var now = DateTime.UtcNow;
        DateTime start, end;

        switch (period.ToLower())
        {
            case "weekly":
                start = now.AddDays(-7);
                end = now;
                break;
            case "monthly":
                start = new DateTime(now.Year, now.Month, 1);
                end = start.AddMonths(1);
                break;
            case "yearly":
                start = new DateTime(now.Year, 1, 1);
                end = start.AddYears(1);
                break;
            case "custom":
                start = startDate ?? now.AddMonths(-1);
                end = endDate ?? now;
                break;
            default:
                start = new DateTime(now.Year, now.Month, 1);
                end = start.AddMonths(1);
                break;
        }

        var transactions = await _db.Transactions
            .Where(t => (t.SenderWalletId == wallet.Id || t.ReceiverWalletId == wallet.Id)
                && t.Status == TransactionStatus.Completed
                && t.CreatedAt >= start && t.CreatedAt < end)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var income = transactions.Where(t => t.ReceiverWalletId == wallet.Id).Sum(t => t.Amount);
        var expense = transactions.Where(t => t.SenderWalletId == wallet.Id).Sum(t => t.Amount);

        var report = new ReportOverviewDto
        {
            TotalIncome = income,
            TotalExpense = expense,
            TotalSavings = income - expense,
            Period = period,
            IncomeExpense = new List<ChartDataPointDto>
            {
                new() { Label = "Income", Value = income, Category = "Income" },
                new() { Label = "Expense", Value = expense, Category = "Expense" }
            },
            CategoryBreakdown = transactions
                .Where(t => t.SenderWalletId == wallet.Id)
                .GroupBy(t => t.Category ?? "Other")
                .Select(g => new ChartDataPointDto { Label = g.Key, Value = g.Sum(t => t.Amount) })
                .OrderByDescending(c => c.Value)
                .ToList(),
            LargestTransactions = transactions
                .OrderByDescending(t => t.Amount)
                .Take(10)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    Status = t.Status.ToString(),
                    Description = t.Description,
                    ReferenceCode = t.ReferenceCode,
                    Direction = t.ReceiverWalletId == wallet.Id ? "IN" : "OUT",
                    CreatedAt = t.CreatedAt
                })
                .ToList()
        };

        return ApiResponseDto<ReportOverviewDto>.Ok(report);
    }

    private async Task<List<RecentActivityDto>> BuildRecentActivitiesAsync(Guid walletId, List<Models.Entities.Transaction> transactions)
    {
        var activities = new List<RecentActivityDto>();

        foreach (var tx in transactions)
        {
            var isSender = tx.SenderWalletId == walletId;
            var icon = tx.Type switch
            {
                TransactionType.Deposit => "💰",
                TransactionType.Withdrawal => "🏦",
                TransactionType.Transfer => isSender ? "📤" : "📥",
                TransactionType.BillPayment => "💡",
                TransactionType.Recharge => "📱",
                TransactionType.QRPayment => "📲",
                TransactionType.MerchantPayment => "🛒",
                _ => "💳"
            };

            var title = tx.Type.ToString();
            var description = tx.Description ?? $"{tx.Type} transaction";

            activities.Add(new RecentActivityDto
            {
                Id = tx.Id,
                Title = title,
                Description = description,
                Amount = tx.Amount,
                Icon = icon,
                Status = tx.Status.ToString(),
                TimeAgo = FormatTimeAgo(tx.CreatedAt),
                CreatedAt = tx.CreatedAt
            });
        }

        return activities;
    }

    private static string FormatTimeAgo(DateTime date)
    {
        var span = DateTime.UtcNow - date;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
        return date.ToString("MMM dd");
    }
}
