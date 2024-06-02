namespace Common.Events;

using System;
using System.Collections.Frozen;
using Marty.Net.Contracts;

public class RefundRequested : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public string RefundId { get; init; } = null!;
    public string PaymentId { get; init; } = null!;

    public int? Amount { get; init; }

    public string? Message { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
