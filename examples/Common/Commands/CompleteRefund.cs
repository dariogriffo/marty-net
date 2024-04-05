namespace Common.Commands;

public class CompleteRefund
{
    public string TransactionId { get; init; } = null!;

    public string OriginalTransactionId { get; init; } = null!;
    public int Amount { get; init; }
}
