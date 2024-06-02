namespace Marty.Net.Tests.Events.Orders;

using System;
using Contracts;

public class OrderAbandoned : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
