namespace EWalletAPI.Models.Enums;

public enum UserRole { User, Admin }

public enum TransactionType
{
    Deposit, Transfer, Withdrawal, BillPayment, Recharge, QRPayment,
    MerchantPayment, CurrencyExchange, Refund, Cashback
}

public enum TransactionStatus { Pending, Completed, Failed, Reversed, Cancelled }

public enum TransactionDirection { In, Out }

public enum WalletStatus { Active, Frozen, Suspended }

public enum AccountType { BankAccount, DebitCard, CreditCard, UpiId, WalletAccount }

public enum PaymentMethodType
{
    Upi, DebitCard, CreditCard, WalletBalance, QRPayment,
    SavedAccount, BankTransfer
}

public enum NotificationType
{
    TransferSuccessful, TransferFailed, MoneyReceived, MoneySent,
    WalletCredited, WalletDebited, CashbackEarned, BillReminder,
    RechargeReminder, SecurityAlert, PasswordChanged, LoginFromNewDevice,
    ProfileUpdated, SystemAnnouncement, MaintenanceAlert, Promotion
}

public enum BillCategory
{
    Electricity, Water, Gas, Broadband, MobileRecharge, DTH,
    Insurance, Subscriptions, GasCylinder, FASTag
}

public enum RechargeType { Mobile, DTH, DataPack, OTT, Electric }

public enum ScheduleFrequency { Once, Daily, Weekly, Monthly, Yearly }

public enum ScheduleStatus { Active, Paused, Cancelled, Completed }

public enum TicketStatus { Open, Pending, Resolved, Closed }

public enum TicketPriority { Low, Medium, High }

public enum FraudRiskLevel { Low, Medium, High, Critical }

public enum FraudAlertStatus { Open, Investigating, Resolved, Dismissed }

public enum GatewayStatus { Pending, Processing, Success, Failed, Cancelled, Refunded }

public enum QRPaymentType { Pay, Receive, Merchant }

public enum AuditAction
{
    Login, Logout, UserUpdate, WalletUpdate, CurrencyUpdate, SettingsChange,
    TransactionApproval, Refund, BlockUser, DeleteUser, FreezeWallet, UnfreezeWallet
}
