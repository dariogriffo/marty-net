namespace Marty.Net.Internal;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wrappers;

internal class PersistentSubscriber : IPersistentSubscriber
{
    private readonly IOptions<MartyConfiguration> _configuration;
    private readonly IHandlesFactory _handlesFactory;
    private readonly ILogger<PersistentSubscriber> _logger;
    private readonly ISerializer _serializer;
    private readonly EventStoreSettings _settings;
    private readonly IInternalPersistentSubscriber _subscriber;

    public PersistentSubscriber(
        ILogger<PersistentSubscriber> logger,
        IInternalPersistentSubscriber subscriber,
        IHandlesFactory handlesFactory,
        ISerializer serializer,
        IOptions<MartyConfiguration> configuration,
        EventStoreSettings settings
    )
    {
        _logger = logger;
        _subscriber = subscriber;
        _handlesFactory = handlesFactory;
        _serializer = serializer;
        _configuration = configuration;
        _settings = settings;
    }

    public Task SubscribeToStream(
        string streamName,
        SubscriptionPosition subscriptionPosition,
        CancellationToken cancellationToken
    )
    {
        if (streamName.Equals("$all"))
        {
            throw new InvalidOperationException("Use SubscribeToAll method instead");
        }

        return _subscriber.Subscribe(
            streamName,
            subscriptionPosition,
            OnEventAppeared,
            cancellationToken
        );
    }

    public Task SubscribeToAll(
        SubscriptionPosition subscriptionPosition,
        CancellationToken cancellationToken
    )
    {
        return _subscriber.Subscribe(
            "$all",
            subscriptionPosition,
            OnEventAppeared,
            cancellationToken
        );
    }

    private async Task OnEventAppeared(
        PersistentSubscription subscription,
        ResolvedEvent resolvedEvent,
        int? retryCount,
        CancellationToken cancellationToken
    )
    {
        if (resolvedEvent.IsResolved && !_settings.ResolveEvents)
        {
            await subscription.Ack(resolvedEvent);
        }

        IServiceScope? scope = default;
        try
        {
            IEvent? deserialize = _serializer.Deserialize(resolvedEvent.Event);
            if (deserialize is null)
            {
                if (_configuration.Value.TreatNonMartyEventsErrors)
                {
                    await ParkEvent(subscription, resolvedEvent);
                }
                else
                {
                    await subscription.Ack(resolvedEvent);
                }

                return;
            }

            IEvent @event = deserialize!;
            _logger.LogTrace("Event {Event} arrived", @event);

            if (!_handlesFactory.TryGetScopeFor(@event, out scope))
            {
                if (_configuration.Value.TreatMissingHandlersAsErrors)
                {
                    await ParkEventAndLogWarning(@event, subscription, resolvedEvent);
                }
                else
                {
                    _logger.LogTrace("Event {Event} ACK with no handler", @event);
                    await subscription.Ack(resolvedEvent);
                }

                return;
            }

            ConsumerContext context = new(resolvedEvent.Event.EventStreamId, retryCount);

            async Task<OperationResult> ExecuteActionsAndHandler()
            {
                PreProcessorWrapper[] preProcessors = _handlesFactory.GetPreProcessorsFor(
                    @event,
                    scope!
                );
                {
                    foreach (PreProcessorWrapper actionInfo in preProcessors)
                    {
                        await actionInfo.Execute(@event, context, cancellationToken);
                    }
                }

                EventHandlerWrapper handler = _handlesFactory.GetHandlerFor(@event, scope!);
                Task<OperationResult> task = handler.Handle(@event, context, cancellationToken);
                OperationResult operationResult = await task;

                PostProcessorWrapper[] processors = _handlesFactory.GetPostProcessorsFor(
                    @event,
                    scope!
                );
                foreach (PostProcessorWrapper processor in processors)
                {
                    operationResult = await processor.Execute(
                        @event,
                        context,
                        operationResult,
                        cancellationToken
                    );
                }

                return operationResult;
            }

            OperationResult result;
            if (
                _handlesFactory.TryGetPipelinesFor(
                    @event,
                    scope!,
                    out List<PipelineBehaviorWrapper>? behaviors
                )
            )
            {
                int length = behaviors!.Count;
                Func<Task<OperationResult>>[] reversed = new Func<Task<OperationResult>>[
                    length + 1
                ];
                behaviors.Reverse();

                reversed[length] = ExecuteActionsAndHandler;

                //Let's build the execution tree
                for (int i = 0; i < length; ++i)
                {
                    PipelineBehaviorWrapper behavior = behaviors[i];
                    Func<Task<OperationResult>> next = reversed[length - i];
                    reversed[length - i - 1] = () =>
                        behavior.Execute(@event, context, next, cancellationToken);
                }

                Func<Task<OperationResult>> func = reversed[0];
                result = await func();
            }
            else
            {
                result = await ExecuteActionsAndHandler();
            }

            _logger.LogTrace("Event {Event} handled with result {Result:G}", @event, result);
            await (
                result switch
                {
                    OperationResult.Park
                        => subscription.Nack(
                            PersistentSubscriptionNakEventAction.Park,
                            string.Empty,
                            resolvedEvent
                        ),
                    OperationResult.ImmediateRetry
                        => subscription.Nack(
                            PersistentSubscriptionNakEventAction.Retry,
                            string.Empty,
                            resolvedEvent
                        ),
                    OperationResult.RetryByAbandon => Task.CompletedTask,
                    _ => subscription.Ack(resolvedEvent)
                }
            );
        }
        catch (UnknownEventAppeared)
        {
            if (_configuration.Value.TreatNonMartyEventsErrors)
            {
                await ParkEvent(subscription, resolvedEvent);
            }
            else
            {
                await subscription.Ack(resolvedEvent);
            }
        }
        catch (Exception ex)
        {
            await ParkEventAndLogError(subscription, ex, resolvedEvent, retryCount);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    private async Task ParkEventAndLogError(
        PersistentSubscription subscription,
        Exception ex,
        ResolvedEvent resolvedEvent,
        int? retryCount
    )
    {
        await subscription.Nack(
            PersistentSubscriptionNakEventAction.Park,
            ex.Message,
            resolvedEvent
        );
        _logger.LogError(
            ex,
            "Error processing event {ResolvedEvent} retryCount {RetryCount}",
            resolvedEvent,
            retryCount
        );
    }

    private async Task ParkEventAndLogWarning(
        IEvent @event,
        PersistentSubscription subscription,
        ResolvedEvent resolvedEvent
    )
    {
        _logger.LogWarning("Handler for event of type {EventType} not found", @event.GetType());
        await subscription.Nack(
            PersistentSubscriptionNakEventAction.Park,
            $"Handler for event of type {@event.GetType()} not found",
            resolvedEvent
        );
    }

    private async Task ParkEvent(PersistentSubscription subscription, ResolvedEvent resolvedEvent)
    {
        await subscription.Nack(
            PersistentSubscriptionNakEventAction.Park,
            "Error deserializing",
            resolvedEvent
        );
    }
}
