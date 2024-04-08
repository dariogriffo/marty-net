namespace Common.Events;

using System;
using System.Collections.Generic;
using Marty.Net.Contracts;

public class PaymentCaptured : IEvent
{
    public string PaymentId { get; init; } = null!;

    public int Amount { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
