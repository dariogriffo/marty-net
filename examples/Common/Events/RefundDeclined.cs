namespace Common.Events;

using System;
using System.Collections.Generic;
using Marty.Net.Contracts;

public class RefundDeclined : IEvent
{
    public string? PaymentId { get; init; }

    public string? TransactionId { get; init; }

    public string? OriginalTransactionId { get; init; }

    public string RefundId { get; init; } = null!;

    public string? Reason { get; init; }

    public DateTime Timestamp { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
