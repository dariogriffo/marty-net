namespace Common.Events;

using System;
using System.Collections.Frozen;
using Marty.Net.Contracts;

public class RefundDeclined : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public string? PaymentId { get; init; }

    public string? TransactionId { get; init; }

    public string? OriginalTransactionId { get; init; }

    public string RefundId { get; init; } = null!;

    public string? Reason { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
