namespace Common.Events;

using System;
using System.Collections.Generic;
using Marty.Net.Contracts;

public class RefundRequested : IEvent
{
    public string RefundId { get; init; } = null!;
    public string PaymentId { get; init; } = null!;

    public int? Amount { get; init; }

    public string? Message { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
