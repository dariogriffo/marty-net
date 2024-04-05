namespace Marty.Net.Contracts.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Microsoft.Extensions.Logging;

internal sealed class AggregatesStore : IAggregateStore
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<AggregatesStore> _logger;
    private readonly IAggregateStreamResolver _streamNameResolver;

    public AggregatesStore(
        IEventStore eventStore,
        IAggregateStreamResolver streamNameResolver,
        ILogger<AggregatesStore> logger
    )
    {
        _eventStore = eventStore;
        _streamNameResolver = streamNameResolver;
        _logger = logger;
    }

    public async Task Create<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        await _eventStore.Save(streamName, aggregate.UncommittedChanges, cancellationToken);

        aggregate.MarkChangesAsCommitted();
    }

    public async Task Update<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        await _eventStore.Append(
            streamName,
            aggregate.UncommittedChanges,
            aggregate.Version - 1,
            cancellationToken
        );

        aggregate.MarkChangesAsCommitted();
    }

    public async Task<T> Hydrate<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        if (aggregate.Version != -1)
        {
            throw new InvalidOperationException(
                $"Aggregate with Id {aggregate.Id} cannot be hydrated since it has events"
            );
        }

        string id = aggregate.Id;
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogTrace("Loading aggregate with id {Id} from stream {StreamName}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        aggregate.LoadFromHistory(data);
        _logger.LogTrace("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> HydrateUntilPosition<T>(
        T aggregate,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogTrace(
            "Loading aggregate with id {Id} from stream {StreamName} until position {StreamPosition}",
            id,
            streamName,
            position
        );
        List<IEvent> data = await _eventStore.ReadStreamUntilPosition(
            streamName,
            position,
            cancellationToken
        );
        aggregate.LoadFromHistory(data);
        _logger.LogTrace(
            "Aggregate with id {Id} loaded from stream {StreamName} until position {StreamPosition}",
            id,
            streamName,
            position
        );
        return aggregate;
    }

    public async Task<T> HydrateFromPosition<T>(
        T aggregate,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogTrace(
            "Loading aggregate with id {Id} from stream {StreamName} from position {StreamPosition}",
            id,
            streamName,
            position
        );
        List<IEvent> data = await _eventStore.ReadStreamFromPosition(
            streamName,
            position,
            cancellationToken
        );
        aggregate.Version = position - 1;
        aggregate.LoadFromHistory(data);
        _logger.LogTrace(
            "Aggregate with id {Id} loaded from stream {StreamName} from position {StreamPosition}",
            id,
            streamName,
            position
        );
        return aggregate;
    }

    public async Task<T> HydrateFromTimestamp<T>(
        T aggregate,
        DateTime timestamp,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogTrace(
            "Loading aggregate with id {Id} from stream {StreamName} from timestamp {Timestamp}",
            id,
            streamName,
            timestamp
        );
        List<IEvent> data = await _eventStore.ReadStreamFromTimestamp(
            streamName,
            timestamp,
            cancellationToken
        );
        aggregate.LoadFromHistory(data);
        _logger.LogTrace(
            "Aggregate with id {Id} loaded from stream {StreamName} from timestamp {Timestamp}",
            id,
            streamName,
            timestamp
        );
        return aggregate;
    }

    public async Task<T> HydrateUntilTimestamp<T>(
        T aggregate,
        DateTime timestamp,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogTrace(
            "Loading aggregate with id {Id} from stream {StreamName} from timestamp {Timestamp}",
            id,
            streamName,
            timestamp
        );
        List<IEvent> data = await _eventStore.ReadStreamUntilTimestamp(
            streamName,
            timestamp,
            cancellationToken
        );
        aggregate.LoadFromHistory(data);
        _logger.LogTrace(
            "Aggregate with id {Id} loaded from stream {StreamName} from timestamp {Timestamp}",
            id,
            streamName,
            timestamp
        );
        return aggregate;
    }

    public Task SubscribeTo<T>(CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        return _eventStore.SubscribeToStreamFromTheStart(
            $"$ce-{_streamNameResolver.CategoryForAggregate<T>()}",
            cancellationToken
        );
    }

    public async Task<T> GetAggregateById<T>(
        string id,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string streamName = _streamNameResolver.StreamForAggregate<T>(id);
        _logger.LogTrace("Loading aggregate with id {Id} from stream {StreamName}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        T aggregate = new();
        aggregate.LoadFromHistory(data);
        _logger.LogTrace("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> GetAggregateFromStream<T>(
        string streamName,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string id = _streamNameResolver.AggregateIdForStream(streamName);
        _logger.LogTrace("Loading aggregate with id {Id} from stream {StreamName}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        T aggregate = new();

        aggregate.LoadFromHistory(data);
        _logger.LogTrace("Aggregate with id {Id} loaded", streamName);
        return aggregate;
    }

    public async Task<T> GetAggregateFromStreamUntilPosition<T>(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        _logger.LogTrace("Loading aggregate from stream {StreamName}", streamName);
        List<IEvent> history = await _eventStore.ReadStreamUntilPosition(
            streamName,
            position,
            cancellationToken
        );
        T aggregate = new();
        aggregate.LoadFromHistory(history);
        _logger.LogTrace(
            "Aggregate with id {Id} loaded from stream {StreamName}",
            aggregate.Id,
            streamName
        );
        return aggregate;
    }

    public async Task<T> GetAggregateFromStreamFromPosition<T>(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        _logger.LogTrace(
            "Loading aggregate from stream {StreamName} from position {StreamPosition}",
            streamName,
            position
        );
        List<IEvent> data = await _eventStore.ReadStreamFromPosition(
            streamName,
            position,
            cancellationToken
        );
        T aggregate = new();
        aggregate.LoadFromHistory(data);
        _logger.LogTrace(
            "Aggregate with id {Id} loaded from stream {StreamName} from position {StreamPosition}",
            aggregate.Id,
            streamName,
            position
        );
        return aggregate;
    }

    public async Task<T> GetAggregateFromStream<T>(
        string streamName,
        IEvent lastEventToLoad,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string id = _streamNameResolver.AggregateIdForStream(streamName);
        _logger.LogTrace("Loading aggregate with id {Id} from stream {StreamName}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        T aggregate = new();
        IEnumerable<IEvent> history = data.TakeWhile(x => x.Timestamp < lastEventToLoad.Timestamp);

        aggregate.LoadFromHistory(history);
        _logger.LogTrace("Aggregate with id {Id} loaded", streamName);
        return aggregate;
    }
}