namespace Marty.Net.Internal;

using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class PersistentSubscriber : IPersistentSubscriber
{
    private readonly IOptions<MartyConfiguration> _configuration;
    private readonly IHandlesFactory _handlesFactory;
    private readonly ILogger<PersistentSubscriber> _logger;
    private readonly ISerializer _serializer;
    private readonly EventStoreSettings _settings;
    private readonly IInternalPersistentSubscriber _subscriber;

    public PersistentSubscriber(
        IInternalPersistentSubscriber subscriber,
        IHandlesFactory handlesFactory,
        ISerializer serializer,
        IOptions<MartyConfiguration> configuration,
        EventStoreSettings settings,
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory.CreateLoggerFor<PersistentSubscriber>();
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

        ExecutionPlan? plan;
        try
        {
            IEvent? @event = _serializer.Deserialize(resolvedEvent.Event);
            if (@event is null)
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

            _logger.LogEventArrived(@event);

            plan = _handlesFactory.TryGetExecutionPlanFor(@event);
            if (plan is null)
            {
                if (_configuration.Value.TreatMissingHandlersAsErrors)
                {
                    await ParkEventAndLogWarning(@event, subscription, resolvedEvent);
                }
                else
                {
                    _logger.LogEventAckWithoutHandler(@event);

                    await subscription.Ack(resolvedEvent);
                }

                return;
            }

            ConsumerContext context = new(resolvedEvent.Event.EventStreamId, retryCount);

            for (int i = plan.Behaviors.Length - 2; i >= 0; i--)
            {
                var index = i;
                plan.Behaviors[i].Next = plan.Behaviors[index + 1].Execute;
            }

            OperationResult result = await plan.Behaviors[0]
                .Execute(@event, context, cancellationToken);

            _logger.LogEventHandled(@event, result);

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

        _logger.LogErrorProcessingEventWithRetry(ex, resolvedEvent, retryCount);
    }

    private async Task ParkEventAndLogWarning(
        IEvent @event,
        PersistentSubscription subscription,
        ResolvedEvent resolvedEvent
    )
    {
        _logger.LogHandlerForEventNotFound(@event);
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
