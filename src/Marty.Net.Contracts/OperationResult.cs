namespace Marty.Net.Contracts;

/// <summary>
///     The result of the handling the <see cref="IEvent" />
/// </summary>
public enum OperationResult
{
    /// <summary>
    ///     The event was handled correctly and should be marked as processed on the subscription.
    /// </summary>
    Ok,

    /// <summary>
    ///     There was a transient error and the event must be retried immediately.
    /// </summary>
    ImmediateRetry,

    /// <summary>
    ///     There was a transient error and the event must be retried at later.
    ///     The message will not be Nacked and EventStore will retry it after the OperationTimeout
    ///     https://developers.eventstore.com/clients/dotnet/5.0/connecting.html#connection-string
    /// </summary>
    RetryByAbandon,

    /// <summary>
    ///     There was an unrecoverable error, and manual intervention is required.
    ///     The event will be parked and can be replied manually later.
    /// </summary>
    Park
}
