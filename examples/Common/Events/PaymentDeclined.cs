namespace Common.Events;

using System;
using System.Collections.Frozen;
using Marty.Net.Contracts;

public class PaymentDeclined : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public string PaymentId { get; init; } = null!;

    public string? Reason { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
