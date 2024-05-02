namespace Marty.Net.Tests.Events.Orders.v2;

using Contracts;
using System;
using System.Collections.Frozen;

public class OrderEventCancelled : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public string? Reason { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
