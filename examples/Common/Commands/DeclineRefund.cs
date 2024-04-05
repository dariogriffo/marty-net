namespace Common.Commands;

public class DeclineRefund
{
    public string PaymentId { get; init; } = null!;

    public string? TransactionId { get; init; }

    public string? OriginalTransactionId { get; init; }

    public string RefundId { get; init; } = null!;

    public string? Reason { get; init; }
}
