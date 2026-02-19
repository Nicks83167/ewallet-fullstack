using EWalletAPI.Models.Entities;
using EWalletAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EWalletAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── User ──────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();

            entity.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(u => u.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Wallet ────────────────────────────────────────────────────────
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("Wallets");
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).ValueGeneratedNever();

            entity.Property(w => w.Balance)
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00m);

            entity.Property(w => w.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(w => w.UpdatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.HasIndex(w => w.UserId)
                .IsUnique()
                .HasDatabaseName("IX_Wallets_UserId");
        });

        // ─── Transaction ───────────────────────────────────────────────────
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedNever();

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(t => t.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(t => t.Description)
                .HasMaxLength(200);

            entity.Property(t => t.ReferenceCode)
                .HasMaxLength(50);

            entity.Property(t => t.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // Sender FK (nullable — deposits have no sender)
            entity.HasOne(t => t.SenderWallet)
                .WithMany(w => w.SentTransactions)
                .HasForeignKey(t => t.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Receiver FK
            entity.HasOne(t => t.ReceiverWallet)
                .WithMany(w => w.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverWalletId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasIndex(t => t.SenderWalletId)
                .HasDatabaseName("IX_Transactions_SenderWalletId");

            entity.HasIndex(t => t.ReceiverWalletId)
                .HasDatabaseName("IX_Transactions_ReceiverWalletId");

            entity.HasIndex(t => t.CreatedAt)
                .HasDatabaseName("IX_Transactions_CreatedAt");

            entity.HasIndex(t => t.ReferenceCode)
                .HasDatabaseName("IX_Transactions_ReferenceCode");
        });
    }
}
