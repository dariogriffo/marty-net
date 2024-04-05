namespace Marty.Net.Contracts.Exceptions;

using System;

/// <summary>
///     An exception representing a failure to append an event to a stream
/// </summary>
public class ErrorAppendingEventsToStream : Exception
{
    /// <summary>
    ///     The constructor
    /// </summary>
    /// <param name="exception">The EventStore exception.</param>
    /// <param name="streamName">The name of the stream.</param>
    public ErrorAppendingEventsToStream(Exception exception, string streamName)
        : base($"Error appending event to stream {streamName}", exception)
    {
        StreamName = streamName;
    }

    /// <summary>
    ///     The name of the stream
    /// </summary>
    public string StreamName { get; }
}
