namespace Marty.Net.Internal;

using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReadEventStore : IReadEventStore
{
    private readonly IConnectionProvider _connectionProvider;
    private readonly IConnectionStrategy _connectionStrategy;
    private readonly ILogger<ReadEventStore> _logger;
    private readonly ISerializer _serializer;
    private readonly EventStoreSettings _settings;

    public ReadEventStore(
        ISerializer serializer,
        IConnectionProvider connectionProvider,
        IConnectionStrategy connectionStrategy,
        EventStoreSettings settings,
        ILoggerFactory? loggerFactory
    )
    {
        _serializer = serializer;
        _logger = loggerFactory.CreateLoggerFor<ReadEventStore>();
        _connectionProvider = connectionProvider;
        _settings = settings;
        _connectionStrategy = connectionStrategy;
    }

    public Task<List<IEvent>> ReadStream(
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.Start;
        _logger.LogReadAllEventsFromStream(streamName);
        return ReadStreamFromPosition(streamName, streamPosition, cancellationToken);
    }

    public Task<List<IEvent>> ReadStreamFromPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.FromInt64(position);
        _logger.LogReadEventsFromStreamFromPosition(streamName, position);
        return ReadStreamFromPosition(streamName, streamPosition, cancellationToken);
    }

    public async Task<List<IEvent>> ReadStreamFromTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    )
    {
        List<IEvent> result = [];
        await _connectionStrategy.Execute(DoRead, cancellationToken);
        return result;

        async Task DoRead(CancellationToken c)
        {
            try
            {
                EventStoreClient.ReadStreamResult events =
                    _connectionProvider.ReadClient.ReadStreamAsync(
                        Direction.Forwards,
                        streamName,
                        StreamPosition.Start,
                        cancellationToken: c
                    );

                await foreach (ResolvedEvent @event in events)
                {
                    if (@event.IsResolved && !_settings.ResolveEvents)
                    {
                        continue;
                    }

                    IEvent? item = _serializer.Deserialize(@event.OriginalEvent);
                    if (item is null)
                    {
                        continue;
                    }

                    if (item.Timestamp < timestamp)
                    {
                        continue;
                    }

                    result.Add(item);
                }

                _logger.LogEventsCountRead(streamName, result.Count);
            }
            catch (StreamNotFoundException)
            {
                throw new StreamNotFound(streamName);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await _connectionProvider.WriteClientDisconnected(_connectionProvider.WriteClient);
                throw new ConnectionFailed();
            }
            catch (Exception ex)
            {
                throw new ErrorReadingFromStream(ex, streamName);
            }
        }
    }

    public async Task<List<IEvent>> ReadStreamUntilPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.Start;

        _logger.LogReadEventsFromStreamUntilPosition(streamName, position);

        List<IEvent> result = [];
        await _connectionStrategy.Execute(DoRead, cancellationToken);
        return result;

        async Task DoRead(CancellationToken c)
        {
            try
            {
                EventStoreClient.ReadStreamResult events =
                    _connectionProvider.ReadClient.ReadStreamAsync(
                        Direction.Forwards,
                        streamName,
                        streamPosition,
                        maxCount: position + 1,
                        cancellationToken: c
                    );

                int i = 0;

                await foreach (ResolvedEvent @event in events)
                {
                    if (@event.IsResolved && !_settings.ResolveEvents)
                    {
                        continue;
                    }

                    IEvent? item = _serializer.Deserialize(@event.OriginalEvent);
                    if (item is null)
                    {
                        continue;
                    }

                    result.Add(item);
                    if (i == position)
                    {
                        break;
                    }

                    ++i;
                }

                _logger.LogEventsCountRead(streamName, result.Count);
            }
            catch (StreamNotFoundException)
            {
                throw new StreamNotFound(streamName);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await _connectionProvider.WriteClientDisconnected(_connectionProvider.WriteClient);
                throw new ConnectionFailed();
            }
            catch (Exception ex)
            {
                throw new ErrorReadingFromStream(ex, streamName);
            }
        }
    }

    public async Task<List<IEvent>> ReadStreamUntilTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.Start;

        _logger.LogReadEventsFromStreamUntilTimestamp(streamName, timestamp);

        List<IEvent> result = [];
        await _connectionStrategy.Execute(DoRead, cancellationToken);

        return result;

        async Task DoRead(CancellationToken c)
        {
            try
            {
                EventStoreClient.ReadStreamResult events =
                    _connectionProvider.ReadClient.ReadStreamAsync(
                        Direction.Forwards,
                        streamName,
                        streamPosition,
                        cancellationToken: c
                    );

                await foreach (ResolvedEvent @event in events)
                {
                    if (@event.IsResolved && !_settings.ResolveEvents)
                    {
                        continue;
                    }

                    IEvent? item = _serializer.Deserialize(@event.OriginalEvent);
                    if (item is null)
                    {
                        continue;
                    }

                    if (item.Timestamp > timestamp)
                    {
                        break;
                    }

                    result.Add(item);
                }

                _logger.LogEventsCountRead(streamName, result.Count);
            }
            catch (StreamNotFoundException)
            {
                throw new StreamNotFound(streamName);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await _connectionProvider.WriteClientDisconnected(_connectionProvider.WriteClient);
                throw new ConnectionFailed();
            }
            catch (Exception ex)
            {
                throw new ErrorReadingFromStream(ex, streamName);
            }
        }
    }

    private async Task<List<IEvent>> ReadStreamFromPosition(
        string streamName,
        StreamPosition streamPosition,
        CancellationToken cancellationToken
    )
    {
        List<IEvent> result = [];
        await _connectionStrategy.Execute(DoRead, cancellationToken);
        return result;

        async Task DoRead(CancellationToken c)
        {
            try
            {
                EventStoreClient.ReadStreamResult events =
                    _connectionProvider.ReadClient.ReadStreamAsync(
                        Direction.Forwards,
                        streamName,
                        streamPosition,
                        cancellationToken: c
                    );

                await foreach (ResolvedEvent @event in events)
                {
                    if (@event.IsResolved && !_settings.ResolveEvents)
                    {
                        continue;
                    }

                    IEvent? item = _serializer.Deserialize(@event.OriginalEvent);
                    if (item is null)
                    {
                        continue;
                    }

                    result.Add(item);
                }

                _logger.LogEventsCountRead(streamName, result.Count);
            }
            catch (StreamNotFoundException)
            {
                throw new StreamNotFound(streamName);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await _connectionProvider.WriteClientDisconnected(_connectionProvider.WriteClient);
                throw new ConnectionFailed();
            }
            catch (Exception ex)
            {
                throw new ErrorReadingFromStream(ex, streamName);
            }
        }
    }
}
