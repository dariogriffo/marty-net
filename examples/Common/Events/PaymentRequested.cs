namespace Common.Events;

using Marty.Net.Contracts;
using System;
using System.Collections.Frozen;

public class PaymentRequested : IEvent
{
    public string CardNumber { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int Amount { get; init; }

    public string Id { get; init; } = null!;

    public DateTimeOffset Timestamp { get; init; }

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
