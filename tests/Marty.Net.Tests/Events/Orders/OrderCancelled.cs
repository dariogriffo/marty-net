namespace Marty.Net.Tests.Events.Orders;

using System;
using System.Collections.Frozen;
using Contracts;

public class OrderCancelled : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
