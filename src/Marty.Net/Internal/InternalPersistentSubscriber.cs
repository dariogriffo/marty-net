namespace Marty.Net.Internal;

using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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
        IConnectionProvider connectionProvider,
        IConnectionStrategy connectionStrategy,
        EventStoreSettings settings,
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory.CreateLoggerFor<InternalPersistentSubscriber>();
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
        string subscriptionGroup =
            _settings.SubscriptionSettings?.SubscriptionGroup
            ?? throw new ArgumentNullException(nameof(subscriptionGroup));

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
                    subscriptionGroup!,
                    settings,
                    cancellationToken: c
                );

                _logger.LogSubscriptionCreated(streamName, subscriptionGroup);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
            {
                // Nothing to do here
            }
            catch (Exception ex)
            {
                _logger.LogErrorCreatingSubscription(ex, streamName, subscriptionGroup);
                throw new SubscriptionFailed(streamName);
            }

            _logger.LogSubscribingToStream(streamName, subscriptionGroup);

            try
            {
                Task<PersistentSubscription> task = streamName.Equals("$all")
                    ? _connectionProvider.PersistentSubscriptionClient.SubscribeToAllAsync(
                        subscriptionGroup,
                        onEventAppeared,
                        OnSubscriptionDropped(streamName, subscriptionPosition),
                        bufferSize: _settings.SubscriptionBufferSize,
                        cancellationToken: cancellationToken
                    )
                    : _connectionProvider.PersistentSubscriptionClient.SubscribeToStreamAsync(
                        streamName,
                        subscriptionGroup,
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
                _logger.LogErrorSubscribingToStream(ex, streamName, subscriptionGroup);

                throw new SubscriptionFailed(streamName);
            }

            _logger.LogSubscribedToStream(streamName, subscriptionGroup, sub.SubscriptionId);
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
                _logger.LogWarningSubscriptionDropped(
                    exception,
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
                _logger.LogErrorSubscriptionDropped(
                    exception,
                    streamName,
                    subscription.SubscriptionId,
                    reason
                );
            }
        };
    }
}
