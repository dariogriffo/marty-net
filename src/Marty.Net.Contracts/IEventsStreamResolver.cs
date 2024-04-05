namespace Marty.Net.Contracts;

/// <summary>
///     An interface required to be implemented in order to know where the events are appended
/// </summary>
public interface IEventsStreamResolver
{
    /// <summary>
    ///     The name of the stream this specific event is stored in.
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent" />.</typeparam>
    /// <param name="event"> .</param>
    /// <returns>The name of the stream.</returns>
    string StreamForEvent<T>(T @event)
        where T : IEvent;
}
