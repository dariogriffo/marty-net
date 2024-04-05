namespace Marty.Net.Tests.Events.Orders.v2;

using System;
using System.Collections.Generic;
using Contracts;

public class OrderRefundRequested : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public decimal Amount { get; init; }

    public bool IsPartial { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
