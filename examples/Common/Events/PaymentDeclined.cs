namespace Common.Events;

using Marty.Net.Contracts;
using System;
using System.Collections.Frozen;

public class PaymentDeclined : IEvent
{
    public string PaymentId { get; init; } = null!;

    public string? Reason { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
