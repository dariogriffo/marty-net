namespace Marty.Net.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using Contracts;

public class OrderRefundRequested : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
