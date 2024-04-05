namespace Marty.Net.Contracts.Exceptions;

using System;

/// <summary>
///     An exception representing that the stream was not found
/// </summary>
public class StreamNotFound : Exception
{
    /// <summary>
    ///     The constructor
    /// </summary>
    /// <param name="streamName">The name of the Stream.</param>
    internal StreamNotFound(string streamName)
        : base($"Stream {streamName} was not found")
    {
        Stream = streamName;
    }

    /// <summary>
    ///     The name of the stream
    /// </summary>
    public string Stream { get; }
}
