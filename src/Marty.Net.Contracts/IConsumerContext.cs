namespace Marty.Net.Contracts;

using System.Collections.Generic;

/// <summary>
///     Context with all the information necessary besides the <see cref="IEvent" /> to be handled
/// </summary>
public interface IConsumerContext
{
    /// <summary>
    ///     The amount of times the event has been processed.
    /// </summary>
    int? RetryCount { get; }

    /// <summary>
    ///     The name of the stream where the event was saved.
    /// </summary>
    string StreamName { get; }

    /// <summary>
    ///     The metadata associated to the event
    /// </summary>
    IReadOnlyDictionary<string, string>? Metadata { get; }
}
