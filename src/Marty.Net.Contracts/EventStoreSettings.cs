﻿namespace Marty.Net.Contracts;

/// <summary>
///     The configuration for the <see cref="IEventStore" />
/// </summary>
public class EventStoreSettings
{
    /// <summary>
    ///     The connection string to access the EventStore
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// </summary>
    public SubscriptionSettings? SubscriptionSettings { get; set; }

    /// <summary>
    ///     The buffer size of the persistent subscription
    /// </summary>
    public int SubscriptionBufferSize { get; set; } = 10;

    /// <summary>
    ///     Configure the behavior of the event store if the subscription gets dropped.
    ///     True for automatic reconnection
    /// </summary>
    public bool ReconnectOnSubscriptionDropped { get; set; } = true;

    /// <summary>
    ///     Configure is the subscription resolve events.
    ///     https://developers.eventstore.com/clients/dotnet/5.0/reading.html#resolvedevent
    /// </summary>
    public bool ResolveEvents { get; set; }
}
