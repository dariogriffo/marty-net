namespace Common.Events;

using System;
using System.Collections.Generic;
using Marty.Net.Contracts;

public class PaymentDeclined : IEvent
{
    public string PaymentId { get; init; } = null!;

    public string? Reason { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
