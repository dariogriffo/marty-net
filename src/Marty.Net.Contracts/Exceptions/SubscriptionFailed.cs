namespace Marty.Net.Contracts.Exceptions;

using System;

/// <summary>
///     An exception representing that the subscription to the stream failed
/// </summary>
public class SubscriptionFailed : Exception
{
    /// <summary>
    ///     The constructor
    /// </summary>
    /// <param name="streamName">.</param>
    internal SubscriptionFailed(string streamName)
        : base($"Subscription to stream {streamName} failed")
    {
        StreamName = streamName;
    }

    /// <summary>
    ///     The name of the stream
    /// </summary>
    public string StreamName { get; }
}
