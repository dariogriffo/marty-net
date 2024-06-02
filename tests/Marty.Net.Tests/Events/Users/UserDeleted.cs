namespace Marty.Net.Tests.Events.Users;

using System;
using System.Collections.Frozen;
using Contracts;

public class UserDeleted : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string UserId { get; init; } = null!;

    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public FrozenDictionary<string, string>? Metadata { get; set; }
}
