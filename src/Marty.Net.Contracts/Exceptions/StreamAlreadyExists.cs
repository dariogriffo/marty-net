namespace Marty.Net.Contracts.Exceptions;

using System;

/// <summary>
///     An exception representing a failure to create a stream with a duplicate id
/// </summary>
public class StreamAlreadyExists : Exception
{
    /// <summary>
    ///     The constructor
    /// </summary>
    /// <param name="streamName">The name of the Stream.</param>
    internal StreamAlreadyExists(string streamName)
        : base($"Trying to create stream {streamName} but already exists")
    {
        Stream = streamName;
    }

    /// <summary>
    ///     The name of the stream
    /// </summary>
    public string Stream { get; }
}
