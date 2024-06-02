namespace Common.Events;

using System;
using System.Collections.Frozen;
using Marty.Net.Contracts;

public class RefundActionCompleted : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public string? TransactionId { get; init; }

    public string? OriginalTransactionId { get; init; }

    public string? Message { get; init; }

    public int Amount { get; set; }

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
