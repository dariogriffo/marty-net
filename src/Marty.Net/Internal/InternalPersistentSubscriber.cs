namespace Marty.Net.Internal;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;

internal sealed class InternalPersistentSubscriber : IInternalPersistentSubscriber
{
    private readonly IConnectionProvider _connectionProvider;
    private readonly IConnectionStrategy _connectionStrategy;
    private readonly ConcurrentDictionary<string, IDisposable> _disposables = new();

    private readonly ILogger<InternalPersistentSubscriber> _logger;
    private readonly EventStoreSettings _settings;

    private Func<
        PersistentSubscription,
        ResolvedEvent,
        int?,
        CancellationToken,
        Task
    >? _onEventAppeared;

    public InternalPersistentSubscriber(
        ILogger<InternalPersistentSubscriber> logger,
        IConnectionProvider connectionProvider,
        IConnectionStrategy connectionStrategy,
        EventStoreSettings settings
    )
    {
        _logger = logger;
        _connectionProvider = connectionProvider;
        _connectionStrategy = connectionStrategy;
        _settings = settings;
    }

    public async Task Subscribe(
        string streamName,
        SubscriptionPosition subscriptionPosition,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> onEventAppeared,
        CancellationToken cancellationToken
    )
    {
#pragma warning disable CA2208
        _ =
            _settings.SubscriptionSettings?.SubscriptionGroup
            ?? throw new ArgumentNullException(
                nameof(_settings.SubscriptionSettings.SubscriptionGroup)
            );
#pragma warning restore CA2208

        _onEventAppeared ??= onEventAppeared;
        if (_disposables.ContainsKey(streamName))
        {
            return;
        }

        await _connectionStrategy.Execute(DoSubscribe, cancellationToken);
        return;

        async Task DoSubscribe(CancellationToken c)
        {
            PersistentSubscription sub = null!;
            try
            {
                IPosition position = streamName.Equals("$all") ? ForAllStream() : ForNonAllStream();

                PersistentSubscriptionSettings settings =
                    new(
                        _settings.ResolveEvents,
                        position,
                        _settings.SubscriptionSettings.ExtraStatistics,
                        _settings.SubscriptionSettings.MessageTimeout,
                        _settings.SubscriptionSettings.MaxRetryCount,
                        _settings.SubscriptionSettings.LiveBufferSize,
                        _settings.SubscriptionSettings.ReadBatchSize,
                        _settings.SubscriptionSettings.HistoryBufferSize,
                        _settings.SubscriptionSettings.CheckPointAfter,
                        _settings.SubscriptionSettings.CheckPointLowerBound,
                        _settings.SubscriptionSettings.CheckPointUpperBound,
                        _settings.SubscriptionSettings.MaxSubscriberCount,
                        _settings.SubscriptionSettings.ConsumerStrategyName
                    );

                await _connectionProvider.PersistentSubscriptionClient.CreateToStreamAsync(
                    streamName,
                    _settings.SubscriptionSettings.SubscriptionGroup!,
                    settings,
                    cancellationToken: c
                );

                _logger.LogTrace(
                    "Created subscription for stream {StreamName} with group {Group}",
                    streamName,
                    _settings.SubscriptionSettings.SubscriptionGroup!
                );
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
            {
                // Nothing to do here
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating subscription to stream {StreamName} with group {Group}",
                    streamName,
                    _settings.SubscriptionSettings.SubscriptionGroup
                );
                throw new SubscriptionFailed(streamName);
            }

            _logger.LogTrace(
                "Subscribing to stream {StreamName} with group {Group}",
                streamName,
                _settings.SubscriptionSettings.SubscriptionGroup
            );

            try
            {
                Task<PersistentSubscription> task = streamName.Equals("$all")
                    ? _connectionProvider.PersistentSubscriptionClient.SubscribeToAllAsync(
                        _settings.SubscriptionSettings.SubscriptionGroup,
                        onEventAppeared,
                        OnSubscriptionDropped(streamName, subscriptionPosition),
                        bufferSize: _settings.SubscriptionBufferSize,
                        cancellationToken: cancellationToken
                    )
                    : _connectionProvider.PersistentSubscriptionClient.SubscribeToStreamAsync(
                        streamName,
                        _settings.SubscriptionSettings.SubscriptionGroup,
                        onEventAppeared,
                        OnSubscriptionDropped(streamName, subscriptionPosition),
                        bufferSize: _settings.SubscriptionBufferSize,
                        cancellationToken: cancellationToken
                    );
                sub = await task;

                _disposables.TryAdd(streamName, sub);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while subscribing to stream {StreamName} with group {Group}",
                    streamName,
                    _settings.SubscriptionSettings.SubscriptionGroup
                );

                throw new SubscriptionFailed(streamName);
            }

            _logger.LogTrace(
                "Subscribed to stream {StreamName} with group {Group} id {SubscriptionId}",
                streamName,
                _settings.SubscriptionSettings.SubscriptionGroup,
                sub.SubscriptionId
            );
        }

        StreamPosition ForNonAllStream()
        {
            return subscriptionPosition switch
            {
                SubscriptionPosition.Start => StreamPosition.Start,
                SubscriptionPosition.End => StreamPosition.End,
                _ => throw new ArgumentException("Position")
            };
        }

        Position ForAllStream()
        {
            return subscriptionPosition switch
            {
                SubscriptionPosition.Start => Position.Start,
                SubscriptionPosition.End => Position.End,
                _ => throw new ArgumentException("Position")
            };
        }
    }

    private Action<
        PersistentSubscription,
        SubscriptionDroppedReason,
        Exception?
    > OnSubscriptionDropped(string streamName, SubscriptionPosition subscriptionPosition)
    {
        return (subscription, reason, exception) =>
        {
            _disposables.TryRemove(streamName, out IDisposable? s);

            if (reason == SubscriptionDroppedReason.Disposed)
            {
                return;
            }

            try
            {
                s?.Dispose();
            }
            catch
            {
                // ignored
            }

            if (_settings.ReconnectOnSubscriptionDropped)
            {
                _logger.LogWarning(
                    exception,
                    "Dropped subscription to stream {StreamName} with id {SubscriptionId}. Reason {Reason}",
                    streamName,
                    subscription.SubscriptionId,
                    reason
                );
                Subscribe(streamName, subscriptionPosition, _onEventAppeared!, default)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                _logger.LogError(
                    exception,
                    "Dropped subscription to stream {StreamName} with id {SubscriptionId}. Reason {Reason}",
                    streamName,
                    subscription.SubscriptionId,
                    reason
                );
            }
        };
    }
}
