namespace Marty.Net.Tests.Events.Orders;

using Contracts;
using System;
using System.Collections.Frozen;

public class OrderRefundRequested : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
