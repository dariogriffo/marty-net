namespace Marty.Net.Contracts;

using System;

/// <summary>
///     An interface that represents an Event.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// The id of the event
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     When the event occurred
    /// </summary>
    DateTimeOffset Timestamp { get; init; }
}
