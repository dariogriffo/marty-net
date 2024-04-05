namespace Marty.Net.Contracts.Exceptions;

using System;

/// <summary>
///     An exception representing a failure to read from a stream
/// </summary>
public class ErrorReadingFromStream : Exception
{
    /// <summary>
    ///     The constructor
    /// </summary>
    /// <param name="exception">The EventStore exception.</param>
    /// <param name="streamName">The name of the stream.</param>
    public ErrorReadingFromStream(Exception exception, string streamName)
        : base($"Error reading from stream {streamName}", exception)
    {
        StreamName = streamName;
    }

    /// <summary>
    ///     The name of the stream
    /// </summary>
    public string StreamName { get; }
}
