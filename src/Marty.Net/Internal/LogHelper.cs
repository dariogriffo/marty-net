namespace Marty.Net.Internal;

using System;
using Contracts;
using global::EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

internal static class LogHelper
{
    internal static ILogger<T> CreateLoggerFor<T>(this ILoggerFactory? factory)
    {
        return factory?.CreateLogger<T>() ?? NullLoggerFactory.Instance.CreateLogger<T>();
    }

    internal static void LogEventsCountRead(this ILogger logger, string streamName, int count)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("{EventsCount} events read on stream {StreamName}", count, streamName);
    }

    internal static void LogReadAllEventsFromStream(this ILogger logger, string streamName)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Reading all events on stream {StreamName}", streamName);
    }

    internal static void LogReadEventsFromStreamFromPosition(
        this ILogger logger,
        string streamName,
        long position
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Reading events on stream {StreamName} from position {StreamPosition}",
            streamName,
            position
        );
    }

    internal static void LogReadEventsFromStreamUntilPosition(
        this ILogger logger,
        string streamName,
        long position
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Reading events on stream {StreamName} until position {StreamPosition}",
            streamName,
            position
        );
    }

    internal static void LogReadEventsFromStreamUntilTimestamp(
        this ILogger logger,
        string streamName,
        DateTimeOffset timestamp
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Reading events on stream {StreamName} until timestamp {Timestamp}",
            streamName,
            timestamp
        );
    }

    internal static void LogEventAdded<T>(this ILogger logger, T @event)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Event {Event} added", @event);
    }

    internal static void LogEventAppended<T>(this ILogger logger, T @event)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Event {Event} appended", @event);
    }

    internal static void LogEventArrived<T>(this ILogger logger, T @event)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Event {Event} arrived", @event);
    }

    internal static void LogEventAckWithoutHandler<T>(this ILogger logger, T @event)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Event {Event} ACK with no handler", @event);
    }

    internal static void LogEventHandled<T>(this ILogger logger, T @event, OperationResult result)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Event {Event} handled with result {Result:G}", @event, result);
    }

    internal static void LogAppendingEventToStream<T>(
        this ILogger logger,
        T @event,
        string streamName
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Appending event {Event} of type {EventType} to stream {StreamName}",
            @event,
            @event!.GetType(),
            streamName
        );
    }

    internal static void LogAppendingEventsCountToStream<T>(
        this ILogger logger,
        T[] @events,
        string streamName
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Appending {EventsCount} events to stream {StreamName}",
            events.Length,
            streamName
        );
    }

    internal static void LogErrorProcessingEventWithRetry(
        this ILogger logger,
        Exception ex,
        ResolvedEvent @event,
        int? retryCount
    )
    {
        if (!logger.IsEnabled(LogLevel.Error))
        {
            return;
        }

        logger.LogError(
            ex,
            "Error processing event {ResolvedEvent} retryCount {RetryCount}",
            @event,
            retryCount
        );
    }

    internal static void LogHandlerForEventNotFound<T>(this ILogger logger, T @event)
    {
        if (!logger.IsEnabled(LogLevel.Warning))
        {
            return;
        }

        logger.LogWarning("Handler for event of type {EventType} not found", @event!.GetType());
    }

    internal static void LogSubscriptionCreated(
        this ILogger logger,
        string streamName,
        string subscriptionGroup
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Created subscription for stream {StreamName} with group {Group}",
            streamName,
            subscriptionGroup
        );
    }

    internal static void LogSubscribingToStream(
        this ILogger logger,
        string streamName,
        string subscriptionGroup
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Subscribing to stream {StreamName} with group {Group}",
            streamName,
            subscriptionGroup
        );
    }

    internal static void LogSubscribedToStream(
        this ILogger logger,
        string streamName,
        string subscriptionGroup,
        string subscriptionId
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Subscribed to stream {StreamName} with group {Group} id {SubscriptionId}",
            streamName,
            subscriptionGroup,
            subscriptionId
        );
    }

    internal static void LogErrorSubscribingToStream(
        this ILogger logger,
        Exception exception,
        string streamName,
        string subscriptionGroup
    )
    {
        if (!logger.IsEnabled(LogLevel.Error))
        {
            return;
        }

        logger.LogError(
            exception,
            "Error while subscribing to stream {StreamName} with group {Group}",
            streamName,
            subscriptionGroup
        );
    }

    internal static void LogErrorCreatingSubscription(
        this ILogger logger,
        Exception exception,
        string streamName,
        string subscriptionGroup
    )
    {
        if (!logger.IsEnabled(LogLevel.Error))
        {
            return;
        }

        logger.LogError(
            exception,
            "Error while creating subscription to stream {StreamName} with group {Group}",
            streamName,
            subscriptionGroup
        );
    }

    internal static void LogWarningSubscriptionDropped(
        this ILogger logger,
        Exception? exception,
        string streamName,
        string subscriptionId,
        SubscriptionDroppedReason reason
    )
    {
        if (!logger.IsEnabled(LogLevel.Warning))
        {
            return;
        }

        logger.LogWarning(
            exception,
            "Dropped subscription to stream {StreamName} with id {SubscriptionId}. Reason {Reason}",
            streamName,
            subscriptionId,
            reason
        );
    }

    internal static void LogErrorSubscriptionDropped(
        this ILogger logger,
        Exception? exception,
        string streamName,
        string subscriptionId,
        SubscriptionDroppedReason reason
    )
    {
        if (!logger.IsEnabled(LogLevel.Error))
        {
            return;
        }

        logger.LogError(
            exception,
            "Dropped subscription to stream {StreamName} with id {SubscriptionId}. Reason {Reason}",
            streamName,
            subscriptionId,
            reason
        );
    }
}
