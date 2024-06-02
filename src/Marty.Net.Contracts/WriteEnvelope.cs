namespace Marty.Net.Contracts;

using System.Collections.Generic;

/// <summary>
/// An envelope containing an <see cref="IEvent"/> and metadata associated to the event
/// </summary>
public record WriteEnvelope<T>
    where T : IEvent
{
    internal WriteEnvelope(T @event, IDictionary<string, string>? metadata = null)
    {
        Event = @event;
        Metadata = metadata;
    }

    /// <summary>
    /// The <see cref="IEvent"/>
    /// </summary>
    public T Event { get; }

    /// <summary>
    ///     The metadata associated to the event
    /// </summary>
    public IDictionary<string, string>? Metadata { get; }
}
