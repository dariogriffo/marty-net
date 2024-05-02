namespace Common.Events;

using Marty.Net.Contracts;
using System;
using System.Collections.Frozen;

public class RefundRequestedRepeated : IEvent
{
    public string PaymentId { get; init; } = null!;

    public string RefundId { get; init; } = null!;

    public string? Message { get; init; }

    public int? Amount { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
