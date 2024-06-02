namespace Marty.Net.Contracts.Internal;

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

internal static class LogHelper
{
    internal static ILogger<T> CreateLoggerFor<T>(this ILoggerFactory? factory)
    {
        return factory?.CreateLogger<T>() ?? NullLoggerFactory.Instance.CreateLogger<T>();
    }

    internal static void LogLoadingAggregate(this ILogger logger, string streamName)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Loading aggregate from stream {StreamName}", streamName);
    }

    internal static void LogAggregateLoaded(this ILogger logger, string id)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace("Aggregate with id {AggregateId} loaded", id);
    }

    internal static void LogLoadingAggregateUntilPosition(
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
            "Loading aggregate from stream {StreamName} until position {StreamPosition}",
            streamName,
            position
        );
    }

    internal static void LogAggregateLoadedUntilPosition(
        this ILogger logger,
        string id,
        string streamName,
        long position
    )
    {
        if (!logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }

        logger.LogTrace(
            "Aggregate with id {AggregateId} loaded from stream {StreamName} until position {StreamPosition}",
            id,
            streamName,
            position
        );
    }

    internal static void LogLoadingAggregateFromPosition(
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
            "Loading aggregate from stream {StreamName} from position {StreamPosition}",
            streamName,
            position
        );
    }

    internal static void LogAggregateLoadedFromTimestamp(
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
            "Aggregate with from stream {StreamName} from timestamp {EventTimestamp}",
            streamName,
            timestamp
        );
    }

    internal static void LogLoadingAggregateFromTimestamp(
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
            "Loading aggregate from stream {StreamName} from timestamp {EventTimestamp}",
            streamName,
            timestamp
        );
    }

    internal static void LogAggregateLoadedUntilTimestamp(
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
            "Aggregate loaded from stream {StreamName} until timestamp {EventTimestamp}",
            streamName,
            timestamp
        );
    }

    internal static void LogLoadingAggregateUntilTimestamp(
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
            "Loading aggregate from stream {StreamName} until timestamp {EventTimestamp}",
            streamName,
            timestamp
        );
    }

    internal static void LogAggregateLoadedFromPosition(
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
            "Aggregate loaded from stream {StreamName} from position {StreamPosition}",
            streamName,
            position
        );
    }
}
