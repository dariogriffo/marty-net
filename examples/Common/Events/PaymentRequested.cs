namespace Common.Events;

using System;
using System.Collections.Frozen;
using Marty.Net.Contracts;

public class PaymentRequested : IEvent
{
    public string CardNumber { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int Amount { get; init; }

    public Guid Id { get; } = Guid.NewGuid();

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
