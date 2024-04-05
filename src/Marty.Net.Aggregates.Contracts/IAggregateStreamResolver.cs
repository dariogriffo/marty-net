namespace Marty.Net.Aggregates.Contracts;

using Net.Contracts;

/// <summary>
///     An interface required to be implemented in order for Marty.Net to work
/// </summary>
public interface IAggregateStreamResolver
{
    /// <summary>
    ///     Returns the name of the stream for the aggregate
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent" />.</typeparam>
    /// <param name="aggregateId">.</param>
    /// <returns>The name of the stream.</returns>
    string StreamForAggregate<T>(string aggregateId)
        where T : Aggregate;

    /// <summary>
    ///     Returns the name of the stream for this aggregate.
    /// </summary>
    /// <typeparam name="T"><see cref="Aggregate" />.</typeparam>
    /// <param name="aggregate">The aggregate.</param>
    /// <returns>The name of the stream.</returns>
    string StreamForAggregate<T>(T aggregate)
        where T : Aggregate;

    /// <summary>
    ///     Returns the id for a given stream name
    /// </summary>
    /// <param name="streamName">The name of the Stream.</param>
    /// <returns>.</returns>
    string AggregateIdForStream(string streamName);

    /// <summary>
    ///     Returns the category for the stream.
    ///     Used to subscribe to all events for the aggregate with the $ce-AggregateName
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    string CategoryForAggregate<T>()
        where T : Aggregate;
}
