namespace Marty.Net.Contracts;

using System;
using System.Collections.Frozen;

/// <summary>
///     An interface that represents an Event.
/// </summary>
public interface IEvent
{
    /// <summary>
    ///     When the event occurred
    /// </summary>
    DateTimeOffset Timestamp { get; init; }

    /// <summary>
    ///     The metadata associated to the event
    /// </summary>
    FrozenDictionary<string, string>? Metadata { get; set; }
}
