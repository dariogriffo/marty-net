namespace Common.Events;

using System;
using System.Collections.Generic;
using Marty.Net.Contracts;

public class RefundActionAccepted : IEvent
{
    public string TransactionId { get; init; } = null!;

    public DateTimeOffset Timestamp { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
