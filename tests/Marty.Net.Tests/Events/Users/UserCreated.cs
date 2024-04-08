namespace Marty.Net.Tests.Events.Users;

using System;
using System.Collections.Generic;
using Contracts;

public class UserCreated : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string UserId { get; init; } = null!;

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
