namespace Common.Aggregates;

public enum PaymentStatus
{
    Requested,
    Declined,
    PartiallyCaptured,
    Captured,
    RefundInitiated,
    RefundAccepted,
    RefundDeclined,
    PartiallyRefunded,
    Refunded
}
