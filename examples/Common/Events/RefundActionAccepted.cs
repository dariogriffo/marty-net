namespace Common.Events;

using System;
using System.Collections.Frozen;
using Marty.Net.Contracts;

public class RefundActionAccepted : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public string TransactionId { get; init; } = null!;

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
