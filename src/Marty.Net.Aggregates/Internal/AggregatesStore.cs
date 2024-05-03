namespace Marty.Net.Contracts.Internal;

using Aggregates.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AggregatesStore : IAggregateStore
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<AggregatesStore> _logger;
    private readonly IAggregateStreamResolver _streamNameResolver;

    public AggregatesStore(
        IEventStore eventStore,
        IAggregateStreamResolver streamNameResolver,
        ILoggerFactory? loggerFactory
    )
    {
        _eventStore = eventStore;
        _streamNameResolver = streamNameResolver;
        _logger = loggerFactory.CreateLoggerFor<AggregatesStore>();
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

        string streamName = _streamNameResolver.StreamForAggregate(aggregate);

        _logger.LogLoadingAggregate(streamName);

        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        aggregate.LoadFromHistory(data);

        _logger.LogAggregateLoaded(streamName);

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
        _logger.LogLoadingAggregateUntilPosition(streamName, position);
        List<IEvent> data = await _eventStore.ReadStreamUntilPosition(
            streamName,
            position,
            cancellationToken
        );
        aggregate.LoadFromHistory(data);
        _logger.LogAggregateLoadedUntilPosition(id, streamName, position);
        return aggregate;
    }

    public async Task<T> HydrateFromPosition<T>(
        T aggregate,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogLoadingAggregateFromPosition(streamName, position);
        List<IEvent> data = await _eventStore.ReadStreamFromPosition(
            streamName,
            position,
            cancellationToken
        );
        aggregate.Version = position - 1;
        aggregate.LoadFromHistory(data);
        _logger.LogAggregateLoadedFromPosition(streamName, position);
        return aggregate;
    }

    public async Task<T> HydrateFromTimestamp<T>(
        T aggregate,
        DateTime timestamp,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);

        _logger.LogLoadingAggregateFromTimestamp(streamName, timestamp);

        List<IEvent> data = await _eventStore.ReadStreamFromTimestamp(
            streamName,
            timestamp,
            cancellationToken
        );
        aggregate.LoadFromHistory(data);

        _logger.LogAggregateLoadedFromTimestamp(streamName, timestamp);

        return aggregate;
    }

    public async Task<T> HydrateUntilTimestamp<T>(
        T aggregate,
        DateTime timestamp,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);

        _logger.LogLoadingAggregateUntilTimestamp(streamName, timestamp);

        List<IEvent> data = await _eventStore.ReadStreamUntilTimestamp(
            streamName,
            timestamp,
            cancellationToken
        );
        aggregate.LoadFromHistory(data);

        _logger.LogAggregateLoadedUntilTimestamp(streamName, timestamp);
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
        _logger.LogLoadingAggregate(streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        T aggregate = new();
        aggregate.LoadFromHistory(data);
        _logger.LogAggregateLoaded(id);
        return aggregate;
    }

    public async Task<T> GetAggregateFromStream<T>(
        string streamName,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string id = _streamNameResolver.AggregateIdForStream(streamName);
        _logger.LogLoadingAggregate(streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        T aggregate = new();

        aggregate.LoadFromHistory(data);
        _logger.LogAggregateLoaded(id);

        return aggregate;
    }

    public async Task<T> GetAggregateFromStreamUntilPosition<T>(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        _logger.LogLoadingAggregate(streamName);
        List<IEvent> history = await _eventStore.ReadStreamUntilPosition(
            streamName,
            position,
            cancellationToken
        );
        T aggregate = new();
        aggregate.LoadFromHistory(history);

        _logger.LogAggregateLoaded(aggregate.Id);

        return aggregate;
    }

    public async Task<T> GetAggregateFromStreamFromPosition<T>(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        _logger.LogLoadingAggregateFromPosition(streamName, position);

        List<IEvent> data = await _eventStore.ReadStreamFromPosition(
            streamName,
            position,
            cancellationToken
        );
        T aggregate = new();
        aggregate.LoadFromHistory(data);
        _logger.LogAggregateLoadedFromPosition(streamName, position);
        return aggregate;
    }

    public async Task<T> GetAggregateFromStream<T>(
        string streamName,
        IEvent lastEventToLoad,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        _logger.LogLoadingAggregate(streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        T aggregate = new();
        IEnumerable<IEvent> history = data.TakeWhile(x => x.Timestamp < lastEventToLoad.Timestamp);

        aggregate.LoadFromHistory(history);
        _logger.LogAggregateLoaded(streamName);
        return aggregate;
    }
}
