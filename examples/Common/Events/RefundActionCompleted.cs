namespace Common.Events;

using System;
using System.Collections.Generic;
using Marty.Net.Contracts;

public class RefundActionCompleted : IEvent
{
    public string? TransactionId { get; init; }

    public string? OriginalTransactionId { get; init; }

    public string? Message { get; init; }

    public int Amount { get; set; }

    public DateTime Timestamp { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
