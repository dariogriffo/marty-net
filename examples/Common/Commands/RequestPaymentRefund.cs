namespace Common.Commands;

public class RequestPaymentRefund
{
    public string PaymentId { get; init; } = null!;

    public int? Amount { get; init; }

    public string? Message { get; init; }
}
