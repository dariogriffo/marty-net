namespace Common.Commands;

public class DeclinePayment
{
    public string PaymentId { get; init; } = null!;

    public string? Reason { get; init; }
}
