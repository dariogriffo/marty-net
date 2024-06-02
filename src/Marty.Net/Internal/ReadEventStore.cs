using System.Collections.ObjectModel;

namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;

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

    public Task<List<ReadEnvelope>> ReadStream(
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.Start;
        _logger.LogReadAllEventsFromStream(streamName);
        return ReadStreamFromPosition(streamName, streamPosition, cancellationToken);
    }

    public Task<List<ReadEnvelope>> ReadStreamFromPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.FromInt64(position);
        _logger.LogReadEventsFromStreamFromPosition(streamName, position);
        return ReadStreamFromPosition(streamName, streamPosition, cancellationToken);
    }

    public async Task<List<ReadEnvelope>> ReadStreamFromTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    )
    {
        List<ReadEnvelope> result = [];
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

                    bool success = _serializer.Deserialize(
                        @event.OriginalEvent,
                        out IEvent? item,
                        out IDictionary<string, string>? metadata
                    );

                    if (!success)
                    {
                        continue;
                    }

                    if (item!.Timestamp < timestamp)
                    {
                        continue;
                    }

                    if (metadata is null)
                    {
                        result.Add(new ReadEnvelope(item));
                    }
                    else
                    {
                        result.Add(
                            new ReadEnvelope(item, new ReadOnlyDictionary<string, string>(metadata))
                        );
                    }
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

    public async Task<List<ReadEnvelope>> ReadStreamUntilPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.Start;

        _logger.LogReadEventsFromStreamUntilPosition(streamName, position);

        List<ReadEnvelope> result = [];
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

                    bool success = _serializer.Deserialize(
                        @event.OriginalEvent,
                        out IEvent? item,
                        out IDictionary<string, string>? metadata
                    );

                    if (!success)
                    {
                        continue;
                    }

                    if (metadata is null)
                    {
                        result.Add(new ReadEnvelope(item!));
                    }
                    else
                    {
                        result.Add(
                            new ReadEnvelope(
                                item!,
                                new ReadOnlyDictionary<string, string>(metadata)
                            )
                        );
                    }
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

    public async Task<List<ReadEnvelope>> ReadStreamUntilTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.Start;

        _logger.LogReadEventsFromStreamUntilTimestamp(streamName, timestamp);

        List<ReadEnvelope> result = [];
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

                    bool success = _serializer.Deserialize(
                        @event.OriginalEvent,
                        out IEvent? item,
                        out IDictionary<string, string>? metadata
                    );
                    if (!success)
                    {
                        continue;
                    }

                    if (item!.Timestamp > timestamp)
                    {
                        break;
                    }

                    if (metadata is null)
                    {
                        result.Add(new ReadEnvelope(item));
                    }
                    else
                    {
                        result.Add(
                            new ReadEnvelope(item, new ReadOnlyDictionary<string, string>(metadata))
                        );
                    }
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

    private async Task<List<ReadEnvelope>> ReadStreamFromPosition(
        string streamName,
        StreamPosition streamPosition,
        CancellationToken cancellationToken
    )
    {
        List<ReadEnvelope> result = [];
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

                    bool success = _serializer.Deserialize(
                        @event.OriginalEvent,
                        out IEvent? item,
                        out IDictionary<string, string>? metadata
                    );
                    if (!success)
                    {
                        continue;
                    }

                    if (metadata is null)
                    {
                        result.Add(new(item!));
                    }
                    else
                    {
                        result.Add(new(item!, new ReadOnlyDictionary<string, string>(metadata)));
                    }
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
