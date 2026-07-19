namespace EWalletAPI.Models.DTOs;

// Dashboard Analytics
public class DashboardAnalyticsDto
{
    public WalletOverviewDto WalletOverview { get; set; } = new();
    public MonthlyAnalyticsDto MonthlyAnalytics { get; set; } = new();
    public QuickInsightsDto QuickInsights { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class WalletOverviewDto
{
    public decimal CurrentBalance { get; set; }
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }
    public string WalletId { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
    public string WalletStatus { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int ProfileCompletionScore { get; set; }
}

public class MonthlyAnalyticsDto
{
    public List<ChartDataPoint> IncomeVsExpense { get; set; } = new();
    public List<ChartDataPoint> WeeklySpending { get; set; } = new();
    public List<ChartDataPoint> CategoryDistribution { get; set; } = new();
    public List<TopTransactionDto> LargestTransactions { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetSavings { get; set; }
    public decimal AverageDailySpending { get; set; }
    public string MostUsedPaymentMethod { get; set; } = string.Empty;
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Color { get; set; }
}

public class TopTransactionDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class RecentActivityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Admin Analytics (separate from feature DTOs to avoid conflicts)
public class AdminAnalyticsOverviewDto
{
    public int TotalUsers { get; set; }
    public int TodaysUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int BlockedUsers { get; set; }
    public int ActiveUsers { get; set; }
    public decimal TotalWalletBalance { get; set; }
    public decimal TodaysRevenue { get; set; }
    public int TodaysTransactions { get; set; }
    public int MonthlyTransactions { get; set; }
    public int PendingPayments { get; set; }
    public int FailedPayments { get; set; }
    public int RefundRequests { get; set; }
    public int FraudAlerts { get; set; }
    public int PendingTickets { get; set; }
}

public class SystemHealthDto
{
    public string ServerStatus { get; set; } = "Healthy";
    public int ActiveSessions { get; set; }
    public decimal ApiResponseTime { get; set; }
    public int ErrorRate { get; set; }
}
