namespace Common.Events;

using Marty.Net.Contracts;
using System;
using System.Collections.Frozen;

public class RefundActionAccepted : IEvent
{
    public string TransactionId { get; init; } = null!;

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
