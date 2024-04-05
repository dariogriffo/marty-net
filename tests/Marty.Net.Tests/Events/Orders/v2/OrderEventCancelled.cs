namespace Marty.Net.Tests.Events.Orders.v2;

using System;
using System.Collections.Generic;
using Contracts;

public class OrderEventCancelled : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public string? Reason { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
