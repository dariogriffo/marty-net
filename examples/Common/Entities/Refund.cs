namespace Common.Entities;

public class Refund
{
    public Refund(string id, string paymentId)
    {
        Id = id;
        PaymentId = paymentId;
    }

    public string Id { get; private set; }

    public string PaymentId { get; private set; }

    public string? TransactionId { get; private set; }

    public void SetTransactionId(string transactionId)
    {
        TransactionId = transactionId;
    }
}
