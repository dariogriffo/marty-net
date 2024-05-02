namespace Common.Events;

using Marty.Net.Contracts;
using System;
using System.Collections.Frozen;

public class RefundActionCompleted : IEvent
{
    public string? TransactionId { get; init; }

    public string? OriginalTransactionId { get; init; }

    public string? Message { get; init; }

    public int Amount { get; set; }

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
