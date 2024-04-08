namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal sealed class EventStore : IEventStore
{
    private readonly IReadEventStore _read;
    private readonly IPersistentSubscriber _subscriber;
    private readonly IWriteEventStore _write;
    private readonly IConnectionProvider _connectionProvider;

    public EventStore(
        IWriteEventStore write,
        IReadEventStore read,
        IPersistentSubscriber subscriber,
        IConnectionProvider connectionProvider
    )
    {
        _write = write;
        _read = read;
        _subscriber = subscriber;
        _connectionProvider = connectionProvider;
    }

    public Task<long> Save<T>(
        string streamName,
        T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        return _write.Save(streamName, @event, cancellationToken);
    }

    public Task<long> Save<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        return _write.Save(streamName, events, cancellationToken);
    }

    public Task<long> Append<T>(
        string streamName,
        T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        return _write.Append(streamName, @event, cancellationToken);
    }

    public Task<long> Append<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        return _write.Append(streamName, events, cancellationToken);
    }

    public Task<long> Append<T>(
        T @event,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        return _write.Append(@event, expectedStreamVersion, cancellationToken);
    }

    public Task<long> Append<T>(
        string streamName,
        T @event,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        return _write.Append(streamName, @event, expectedStreamVersion, cancellationToken);
    }

    public Task<long> Append<T>(
        string streamName,
        T[] events,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        return _write.Append(streamName, events, expectedStreamVersion, cancellationToken);
    }

    public Task<long> Append<T>(T @event, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        return _write.Append(@event, cancellationToken);
    }

    public Task<List<IEvent>> ReadStreamFromPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
    {
        return _read.ReadStreamFromPosition(streamName, position, cancellationToken);
    }

    public Task<List<IEvent>> ReadStreamUntilPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
    {
        return _read.ReadStreamUntilPosition(streamName, position, cancellationToken);
    }

    public Task<List<IEvent>> ReadStreamUntilTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    )
    {
        return _read.ReadStreamUntilTimestamp(streamName, timestamp, cancellationToken);
    }

    public Task<List<IEvent>> ReadStreamFromTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken
    )
    {
        return _read.ReadStreamFromTimestamp(streamName, timestamp, cancellationToken);
    }

    public Task<List<IEvent>> ReadStream(
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        return _read.ReadStream(streamName, cancellationToken);
    }

    public Task SubscribeToStreamFromTheEnd(
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        return _subscriber.SubscribeToStream(
            streamName,
            SubscriptionPosition.End,
            cancellationToken
        );
    }

    public Task SubscribeToStreamFromTheStart(
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        return _subscriber.SubscribeToStream(
            streamName,
            SubscriptionPosition.Start,
            cancellationToken
        );
    }

    public Task SubscribeToStream(
        string streamName,
        SubscriptionPosition subscriptionPosition,
        CancellationToken cancellationToken = default
    )
    {
        return _subscriber.SubscribeToStream(streamName, subscriptionPosition, cancellationToken);
    }

    public Task SubscribeToAll(
        SubscriptionPosition subscriptionPosition,
        CancellationToken cancellationToken = default
    )
    {
        return _subscriber.SubscribeToAll(subscriptionPosition, cancellationToken);
    }

    public Task SubscribeToAllFromTheStart(CancellationToken cancellationToken = default)
    {
        return _subscriber.SubscribeToAll(SubscriptionPosition.Start, cancellationToken);
    }

    public Task SubscribeToAllFromTheEnd(CancellationToken cancellationToken = default)
    {
        return _subscriber.SubscribeToAll(SubscriptionPosition.End, cancellationToken);
    }

    public ValueTask StopConnections() => _connectionProvider.StopConnections();
}
