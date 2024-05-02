namespace Marty.Net.Tests.Events.Orders;

using Contracts;
using System;
using System.Collections.Frozen;

public class OrderDelivered : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
