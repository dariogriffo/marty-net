namespace Common.Events;

using System;
using System.Collections.Generic;
using Marty.Net.Contracts;

public class PaymentRequested : IEvent
{
    public string CardNumber { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int Amount { get; init; }

    public string Id { get; init; } = null!;

    public DateTime Timestamp { get; init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
