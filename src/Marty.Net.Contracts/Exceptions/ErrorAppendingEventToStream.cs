namespace Marty.Net.Contracts.Exceptions;

using System;

/// <summary>
///     An exception representing a failure to append an event to a stream
/// </summary>
public class ErrorAppendingEventToStream : Exception
{
    /// <summary>
    ///     The constructor
    /// </summary>
    /// <param name="exception">The EventStore exception.</param>
    /// <param name="event">The <see cref="IEvent" />.</param>
    /// <param name="streamName">The name of the stream.</param>
    public ErrorAppendingEventToStream(Exception exception, IEvent @event, string streamName)
        : base($"Error appending event to stream {streamName}", exception)
    {
        Event = @event;
        StreamName = streamName;
    }

    /// <summary>
    ///     The name of the stream
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    ///     The <see cref="IEvent" /> tried to be appended to the stream
    /// </summary>
    public IEvent Event { get; }
}
