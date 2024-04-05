namespace Marty.Net.Contracts;

using System;
using System.Collections.Generic;

/// <summary>
///     An interface that represents an Event.
/// </summary>
public interface IEvent
{
    /// <summary>
    ///     When the event occurred
    /// </summary>
    DateTime Timestamp { get; init; }

    /// <summary>
    ///     The metadata associated to the event
    /// </summary>
    IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
