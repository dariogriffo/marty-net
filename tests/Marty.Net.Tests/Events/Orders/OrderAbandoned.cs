namespace Marty.Net.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using Contracts;

public class OrderAbandoned : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
