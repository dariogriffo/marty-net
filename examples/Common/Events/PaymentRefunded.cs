namespace Common.Events;

using Marty.Net.Contracts;
using System;
using System.Collections.Frozen;

public class PaymentRefunded : IEvent
{
    public string PaymentId { get; init; } = null!;

    public int? Amount { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
