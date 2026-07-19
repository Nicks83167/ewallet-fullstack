using EWalletAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<CurrencyConversionLog> CurrencyConversionLogs => Set<CurrencyConversionLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<LinkedAccount> LinkedAccounts => Set<LinkedAccount>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<BillPayment> BillPayments => Set<BillPayment>();
    public DbSet<RechargeRecord> RechargeRecords => Set<RechargeRecord>();
    public DbSet<ScheduledPayment> ScheduledPayments => Set<ScheduledPayment>();
    public DbSet<QRPayment> QRPayments => Set<QRPayment>();
    public DbSet<DemoGatewayTransaction> DemoGatewayTransactions => Set<DemoGatewayTransaction>();
    public DbSet<MerchantCategory> MerchantCategories => Set<MerchantCategory>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<MerchantPayment> MerchantPayments => Set<MerchantPayment>();
    public DbSet<DemoBank> DemoBanks => Set<DemoBank>();
    public DbSet<DemoBankAccount> DemoBankAccounts => Set<DemoBankAccount>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<FraudAlert> FraudAlerts => Set<FraudAlert>();
    public DbSet<FraudHistory> FraudHistories => Set<FraudHistory>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportReply> SupportReplies => Set<SupportReply>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AdminLog> AdminLogs => Set<AdminLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AnalyticsSnapshot> AnalyticsSnapshots => Set<AnalyticsSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(u => u.Phone).HasMaxLength(20);
            entity.Property(u => u.PanNumber).HasMaxLength(20);
            entity.Property(u => u.AadhaarNumber).HasMaxLength(20);
            entity.HasOne(u => u.Wallet).WithOne(w => w.User).HasForeignKey<Wallet>(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("Wallets");
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).ValueGeneratedNever();
            entity.Property(w => w.Balance).HasPrecision(18, 2);
            entity.Property(w => w.CurrencyCode).HasMaxLength(10).HasDefaultValue("INR");
            entity.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(w => w.DailyLimit).HasPrecision(18, 2);
            entity.Property(w => w.MonthlyLimit).HasPrecision(18, 2);
            entity.HasIndex(w => w.UserId).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedNever();
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.OriginalAmount).HasPrecision(18, 2);
            entity.Property(t => t.ConvertedAmount).HasPrecision(18, 2);
            entity.Property(t => t.ExchangeRate).HasPrecision(18, 6);
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(t => t.Direction).HasConversion<string>().HasMaxLength(10);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.CurrencyCode).HasMaxLength(10);
            entity.Property(t => t.Category).HasMaxLength(50);
            entity.Property(t => t.PaymentMethod).HasMaxLength(50);
            entity.Property(t => t.MerchantName).HasMaxLength(100);
            entity.HasOne(t => t.SenderWallet).WithMany(w => w.SentTransactions).HasForeignKey(t => t.SenderWalletId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.ReceiverWallet).WithMany(w => w.ReceivedTransactions).HasForeignKey(t => t.ReceiverWalletId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.Merchant).WithMany(m => m.Transactions).HasForeignKey(t => t.MerchantId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(t => t.CreatedAt);
            entity.HasIndex(t => t.ReferenceCode);
        });

        ConfigureSimpleEntity<Currency>(modelBuilder, "Currencies", e => e.Property(c => c.Code).HasMaxLength(10));
        ConfigureSimpleEntity<ExchangeRate>(modelBuilder, "ExchangeRates");
        ConfigureSimpleEntity<CurrencyConversionLog>(modelBuilder, "CurrencyConversionLogs");

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Type).HasConversion<string>().HasMaxLength(40);
            entity.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });

        modelBuilder.Entity<LinkedAccount>(entity =>
        {
            entity.ToTable("LinkedAccounts");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.AccountType).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(l => l.User).WithMany(u => u.LinkedAccounts).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.ToTable("PaymentMethods");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.MethodType).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(p => p.User).WithMany(u => u.PaymentMethods).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("Beneficiaries");
            entity.HasKey(b => b.Id);
            entity.HasOne(b => b.User).WithMany(u => u.Beneficiaries).HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureSimpleEntity<BillPayment>(modelBuilder, "BillPayments", e =>
        {
            e.Property(b => b.Category).HasConversion<string>().HasMaxLength(30);
            e.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        });
        ConfigureSimpleEntity<RechargeRecord>(modelBuilder, "RechargeRecords", e =>
        {
            e.Property(r => r.RechargeType).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        });
        ConfigureSimpleEntity<ScheduledPayment>(modelBuilder, "ScheduledPayments", e =>
        {
            e.Property(s => s.Frequency).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        });
        ConfigureSimpleEntity<QRPayment>(modelBuilder, "QRPayments", e =>
        {
            e.Property(q => q.PaymentType).HasConversion<string>().HasMaxLength(20);
            e.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
        });
        ConfigureSimpleEntity<DemoGatewayTransaction>(modelBuilder, "DemoGatewayTransactions", e => e.Property(g => g.Status).HasConversion<string>().HasMaxLength(20));

        modelBuilder.Entity<MerchantCategory>(entity => { entity.ToTable("MerchantCategories"); entity.HasKey(m => m.Id); });
        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.ToTable("Merchants");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.TotalRevenue).HasPrecision(18, 2);
            entity.HasOne(m => m.Category).WithMany().HasForeignKey(m => m.CategoryId);
        });
        modelBuilder.Entity<MerchantPayment>(entity =>
        {
            entity.ToTable("MerchantPayments");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Amount).HasPrecision(18, 2);
            entity.HasOne(m => m.Merchant).WithMany(mr => mr.Payments).HasForeignKey(m => m.MerchantId);
        });

        modelBuilder.Entity<DemoBank>(entity =>
        {
            entity.ToTable("DemoBanks");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.TotalRevenue).HasPrecision(18, 2);
        });
        modelBuilder.Entity<DemoBankAccount>(entity =>
        {
            entity.ToTable("DemoBankAccounts");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Balance).HasPrecision(18, 2);
            entity.HasOne(a => a.Bank).WithMany(b => b.Accounts).HasForeignKey(a => a.BankId);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasOne(d => d.User).WithMany(u => u.Devices).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Sessions");
            entity.HasOne(s => s.User).WithMany(u => u.Sessions).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.ToTable("LoginHistories");
            entity.HasOne(l => l.User).WithMany(u => u.LoginHistories).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureSimpleEntity<FraudAlert>(modelBuilder, "FraudAlerts", e =>
        {
            e.Property(f => f.RiskLevel).HasConversion<string>().HasMaxLength(20);
            e.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
        });
        ConfigureSimpleEntity<FraudHistory>(modelBuilder, "FraudHistories");

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("SupportTickets");
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(t => t.User).WithMany(u => u.SupportTickets).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<SupportReply>(entity =>
        {
            entity.ToTable("SupportReplies");
            entity.HasOne(r => r.Ticket).WithMany(t => t.Replies).HasForeignKey(r => r.TicketId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureSimpleEntity<AuditLog>(modelBuilder, "AuditLogs", e => e.Property(a => a.Action).HasConversion<string>().HasMaxLength(40));
        ConfigureSimpleEntity<AdminLog>(modelBuilder, "AdminLogs");
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable("SystemSettings");
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.Key).IsUnique();
        });
        ConfigureSimpleEntity<AnalyticsSnapshot>(modelBuilder, "AnalyticsSnapshots");
    }

    private static void ConfigureSimpleEntity<T>(ModelBuilder modelBuilder, string table, Action<Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T>>? configure = null) where T : class
    {
        modelBuilder.Entity<T>(entity =>
        {
            entity.ToTable(table);
            entity.HasKey("Id");
            configure?.Invoke(entity);
        });
    }
}
