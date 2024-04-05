namespace Marty.Net.Contracts.Exceptions;

using System;

/// <summary>
///     An exception representing a different version than the expected while performing an append to the stream
/// </summary>
public class MismatchExpectedVersion : Exception
{
    /// <summary>
    ///     The constructor
    /// </summary>
    /// <param name="streamName">The name of the Stream.</param>
    /// <param name="expected">.</param>
    /// <param name="actual">.</param>
    internal MismatchExpectedVersion(string streamName, long expected, long actual)
        : base($"Stream {streamName} expected to have version {expected} but found {actual}")
    {
        Stream = streamName;
    }

    /// <summary>
    ///     The name of the stream
    /// </summary>
    public string Stream { get; }
}
