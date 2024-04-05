namespace Marty.Net.Tests.Events.Users;

using System;
using System.Collections.Generic;
using Contracts;

public class UserActivated : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string UserId { get; init; } = null!;

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
