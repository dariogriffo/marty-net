using System.Collections.Generic;

namespace Marty.Net.Contracts;

/// <summary>
/// An envelope containing an <see cref="IEvent"/> and metadata associated to the event
/// </summary>
public record ReadEnvelope
{
    internal ReadEnvelope(IEvent @event, IReadOnlyDictionary<string, string>? metadata = null)
    {
        Event = @event;
        Metadata = metadata;
    }

    /// <summary>
    /// The <see cref="IEvent"/>
    /// </summary>
    public IEvent Event { get; }

    /// <summary>
    ///     The metadata associated to the event
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; }
}
