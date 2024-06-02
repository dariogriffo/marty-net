namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Wrappers;

internal sealed class WriteEventStore : IWriteEventStore
{
    private readonly IConnectionProvider _connectionProvider;
    private readonly IConnectionStrategy _connectionStrategy;

    private readonly IEventsStreamResolver _eventsStreamResolver;
    private readonly ILogger<WriteEventStore> _logger;
    private readonly ISerializer _serializer;
    private readonly IHandlesFactory _handlesFactory;
    private readonly IServiceProvider _serviceProvider;

    public WriteEventStore(
        ISerializer serializer,
        IEventsStreamResolver eventsStreamResolver,
        IConnectionProvider connectionProvider,
        IConnectionStrategy connectionStrategy,
        IHandlesFactory handlesFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory? loggerFactory
    )
    {
        _serializer = serializer;
        _eventsStreamResolver = eventsStreamResolver;
        _logger = loggerFactory.CreateLoggerFor<WriteEventStore>();
        _connectionProvider = connectionProvider;
        _connectionStrategy = connectionStrategy;
        _handlesFactory = handlesFactory;
        _serviceProvider = serviceProvider;
    }

    public Task<long> Save<T>(
        string streamName,
        T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => Save(streamName, new WriteEnvelope<T>(@event), cancellationToken);

    public Task<long> Save<T>(
        string streamName,
        WriteEnvelope<T> envelope,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogAppendingEventToStream(envelope, streamName);

        StreamState expectedState = StreamState.NoStream;
        return SaveEvent(streamName, envelope, expectedState, null, cancellationToken);
    }

    public Task<long> Save<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent =>
        Save(streamName, events.Select(e => new WriteEnvelope<T>(e)).ToArray(), cancellationToken);

    public async Task<long> Save<T>(
        string streamName,
        WriteEnvelope<T>[] envelopes,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        bool hasBefore = _handlesFactory.TryGetPreAppendEventActionFor(
            envelopes.First().Event,
            _serviceProvider,
            out List<PreAppendEventActionWrapper>? beforeActions
        );

        bool hasAfter = _handlesFactory.TryGetPostAppendEventActionsFor(
            envelopes.First().Event,
            _serviceProvider,
            out List<PostAppendEventActionWrapper>? afterActions
        );

        EventData[] data = new EventData[envelopes.Length];
        for (int index = 0; index < envelopes.Length; index++)
        {
            WriteEnvelope<T> envelope = envelopes[index];
            T @event = envelope.Event;
            if (hasBefore)
            {
                foreach (PreAppendEventActionWrapper action in beforeActions!)
                {
                    await action.Execute(envelope, cancellationToken);
                }
            }

            _logger.LogAppendingEventToStream(@event, streamName);

            data[index] = _serializer.Serialize(envelope);
        }

        StreamState expectedState = StreamState.NoStream;
        long result = await SaveEventsWithRetryStrategy(
            streamName,
            data,
            expectedState,
            null,
            cancellationToken
        );

        foreach (WriteEnvelope<T> envelope in envelopes)
        {
            if (hasAfter)
            {
                foreach (PostAppendEventActionWrapper action in afterActions!)
                {
                    Task task = action.Execute(envelope, cancellationToken);
                    await task;
                }
            }

            _logger.LogEventAdded(envelope.Event);
        }

        return result;
    }

    public Task<long> Append<T>(
        string streamName,
        T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => Append(streamName, new WriteEnvelope<T>(@event), cancellationToken);

    public Task<long> Append<T>(
        string streamName,
        WriteEnvelope<T> envelop,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogAppendingEventToStream(envelop, streamName);

        StreamState expectedState = StreamState.StreamExists;
        return SaveEvent(streamName, envelop, expectedState, null, cancellationToken);
    }

    public Task<long> Append<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent =>
        Append(
            streamName,
            events.Select(e => new WriteEnvelope<T>(e)).ToArray(),
            cancellationToken
        );

    public async Task<long> Append<T>(
        string streamName,
        WriteEnvelope<T>[] envelopes,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        bool hasBefore = _handlesFactory.TryGetPreAppendEventActionFor(
            envelopes.First().Event,
            _serviceProvider,
            out List<PreAppendEventActionWrapper>? beforeActions
        );

        _logger.LogAppendingEventsCountToStream(envelopes, streamName);

        EventData[] data = new EventData[envelopes.Length];
        for (int index = 0; index < envelopes.Length; index++)
        {
            WriteEnvelope<T> envelope = envelopes[index];
            T @event = envelope.Event;
            if (hasBefore)
            {
                foreach (PreAppendEventActionWrapper action in beforeActions!)
                {
                    await action.Execute(envelope, cancellationToken);
                }
            }

            _logger.LogAppendingEventToStream(@event, streamName);

            data[index] = _serializer.Serialize(envelope);
        }

        StreamState expectedState = StreamState.StreamExists;
        long result = await SaveEventsWithRetryStrategy(
            streamName,
            data,
            expectedState,
            null,
            cancellationToken
        );

        bool hasAfter = _handlesFactory.TryGetPostAppendEventActionsFor(
            envelopes.First().Event,
            _serviceProvider,
            out List<PostAppendEventActionWrapper>? afterActions
        );

        foreach (WriteEnvelope<T> envelope in envelopes)
        {
            IEvent @event = envelope.Event;
            _logger.LogEventAdded(@event);
            if (!hasAfter)
            {
                continue;
            }

            foreach (PostAppendEventActionWrapper action in afterActions!)
            {
                await action.Execute(envelope, cancellationToken);
            }
        }

        return result;
    }

    public Task<long> Append<T>(T @event, CancellationToken cancellationToken = default)
        where T : IEvent => Append(new WriteEnvelope<T>(@event), cancellationToken);

    public Task<long> Append<T>(
        WriteEnvelope<T> envelope,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        string streamName = _eventsStreamResolver.StreamForEvent(envelope.Event);
        _logger.LogAppendingEventToStream(envelope, streamName);
        StreamState expectedState = StreamState.StreamExists;
        return SaveEvent(streamName, envelope, expectedState, null, cancellationToken);
    }

    public Task<long> Append<T>(
        string streamName,
        T @event,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent =>
        Append(streamName, new WriteEnvelope<T>(@event), expectedStreamVersion, cancellationToken);

    public Task<long> Append<T>(
        string streamName,
        WriteEnvelope<T> envelope,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogAppendingEventToStream(envelope, streamName);

        StreamState expectedState = StreamState.StreamExists;
        return SaveEvent(
            streamName,
            envelope,
            expectedState,
            expectedStreamVersion,
            cancellationToken
        );
    }

    public Task<long> Append<T>(
        string streamName,
        T[] events,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent =>
        Append(
            streamName,
            events.Select(e => new WriteEnvelope<T>(e)).ToArray(),
            expectedStreamVersion,
            cancellationToken
        );

    public async Task<long> Append<T>(
        string streamName,
        WriteEnvelope<T>[] envelopes,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogAppendingEventsCountToStream(envelopes, streamName);

        bool hasBefore = _handlesFactory.TryGetPreAppendEventActionFor(
            envelopes.First().Event,
            _serviceProvider,
            out List<PreAppendEventActionWrapper>? beforeActions
        );

        EventData[] data = new EventData[envelopes.Length];
        for (int index = 0; index < envelopes.Length; index++)
        {
            WriteEnvelope<T> envelope = envelopes[index];
            T @event = envelope.Event;
            if (hasBefore)
            {
                foreach (PreAppendEventActionWrapper action in beforeActions!)
                {
                    await action.Execute(envelope, cancellationToken);
                }
            }

            data[index] = _serializer.Serialize(envelope);
        }

        StreamState expectedState = StreamState.StreamExists;
        long result = await SaveEventsWithRetryStrategy(
            streamName,
            data,
            expectedState,
            expectedStreamVersion,
            cancellationToken
        );

        bool hasAfter = _handlesFactory.TryGetPostAppendEventActionsFor(
            envelopes.First().Event,
            _serviceProvider,
            out List<PostAppendEventActionWrapper>? afterActions
        );

        for (int index = 0; index < envelopes.Length; index++)
        {
            WriteEnvelope<T> envelope = envelopes[index];
            T @event = envelope.Event;
            _logger.LogEventAdded(@event);
            if (hasAfter)
            {
                foreach (PostAppendEventActionWrapper action in afterActions!)
                {
                    await action.Execute(envelope, cancellationToken);
                }
            }
        }

        return result;
    }

    public Task<long> Append<T>(
        T @event,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent =>
        Append(new WriteEnvelope<T>(@event), expectedStreamVersion, cancellationToken);

    public Task<long> Append<T>(
        WriteEnvelope<T> envelope,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        string streamName = _eventsStreamResolver.StreamForEvent(envelope.Event);
        return Append(streamName, envelope, expectedStreamVersion, cancellationToken);
    }

    private async Task<long> SaveEvent<T>(
        string streamName,
        WriteEnvelope<T> envelope,
        StreamState expectedState,
        long? expectedStreamVersion,
        CancellationToken cancellationToken
    )
        where T : IEvent
    {
        IEvent @event = envelope.Event;
        bool hasBefore = _handlesFactory.TryGetPreAppendEventActionFor(
            @event,
            _serviceProvider,
            out List<PreAppendEventActionWrapper>? beforeActions
        );
        bool hasAfter = _handlesFactory.TryGetPostAppendEventActionsFor(
            @event,
            _serviceProvider,
            out List<PostAppendEventActionWrapper>? afterActions
        );
        if (hasBefore)
        {
            foreach (PreAppendEventActionWrapper action in beforeActions!)
            {
                await action.Execute(envelope, cancellationToken);
            }
        }

        EventData[] data = [_serializer.Serialize(envelope)];
        long result = await SaveEventsWithRetryStrategy(
            streamName,
            data,
            expectedState,
            expectedStreamVersion,
            cancellationToken
        );

        _logger.LogEventAppended(envelope);

        if (hasAfter)
        {
            foreach (PostAppendEventActionWrapper action in afterActions!)
            {
                await action.Execute(envelope, cancellationToken);
            }
        }

        return result;
    }

    private Task<long> SaveEventsWithRetryStrategy(
        string streamName,
        EventData[] data,
        StreamState expectedState,
        long? expectedStreamVersion,
        CancellationToken cancellationToken
    )
    {
        return _connectionStrategy.Execute(DoAppend, cancellationToken);

        async Task<long> DoAppend(CancellationToken c)
        {
            try
            {
                IWriteResult result;
                if (expectedStreamVersion.HasValue)
                {
                    result = await _connectionProvider.WriteClient.AppendToStreamAsync(
                        streamName,
                        StreamRevision.FromInt64(expectedStreamVersion.Value),
                        data,
                        cancellationToken: c
                    );
                }
                else
                {
                    result = await _connectionProvider.WriteClient.AppendToStreamAsync(
                        streamName,
                        expectedState,
                        data,
                        cancellationToken: c
                    );
                }

                return result.NextExpectedStreamRevision.ToInt64();
            }
            catch (WrongExpectedVersionException ex)
                when (expectedState == StreamState.NoStream
                    && ex.ExpectedStreamRevision == StreamRevision.None
                )
            {
                throw new StreamAlreadyExists(streamName);
            }
            catch (WrongExpectedVersionException ex) when (expectedStreamVersion.HasValue)
            {
                throw new MismatchExpectedVersion(
                    streamName,
                    expectedStreamVersion.Value,
                    ex.ActualStreamRevision.ToInt64()
                );
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await _connectionProvider.WriteClientDisconnected(_connectionProvider.WriteClient);
                throw new ConnectionFailed();
            }
            catch (Exception ex)
            {
                throw new ErrorAppendingEventsToStream(ex, streamName);
            }
        }
    }
}
