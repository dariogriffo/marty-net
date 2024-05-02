namespace Common.Events;

using Marty.Net.Contracts;
using System;
using System.Collections.Frozen;

public class RefundRequested : IEvent
{
    public string RefundId { get; init; } = null!;
    public string PaymentId { get; init; } = null!;

    public int? Amount { get; init; }

    public string? Message { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
