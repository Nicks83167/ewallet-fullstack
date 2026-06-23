namespace EWalletAPI.Models.Enums;

public enum UserRole
{
    User,
    Admin
}

public enum TransactionType
{
    Deposit,
    Transfer,
    Withdrawal
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Reversed
}

public enum TransactionDirection
{
    In,
    Out
}
