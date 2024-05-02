namespace Marty.Net.Aggregates.Contracts;

using Net.Contracts.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     The interface that gives access to https://github.com/EventStore/EventStore
/// </summary>
public interface IAggregateStore
{
    /// <summary>
    ///     Saves the aggregate root to the Event Store, might throw an exception if the stream for the aggregate root exists
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="Aggregate" />
    /// </typeparam>
    /// <param name="aggregate">The aggregate to be saved.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task Create<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;

    /// <summary>
    ///     Saves the aggregate root to the Event Store
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="Aggregate" />
    /// </typeparam>
    /// <param name="aggregate">The aggregate to be saved.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task Update<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;

    /// <summary>
    ///     Gets the aggregate from the Store.
    ///     The Aggregate must have a parameterless constructor
    /// </summary>
    /// <param name="id">The aggregate id.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> GetAggregateById<T>(string id, CancellationToken cancellationToken = default)
        where T : Aggregate, new();

    /// <summary>
    ///     Gets the aggregate from the Store.
    ///     The Aggregate must have a parameterless constructor
    /// </summary>
    /// <param name="streamName">The stream id.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> GetAggregateFromStream<T>(
        string streamName,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new();

    /// <summary>
    ///     Gets the aggregate from the Store.
    ///     The Aggregate must have a parameterless constructor
    /// </summary>
    /// <param name="streamName">The stream id.</param>
    /// <param name="position">The position of the last event to load into the aggregate.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> GetAggregateFromStreamUntilPosition<T>(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new();

    /// <summary>
    ///     Hydrates the aggregate root from the Event Store
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> Hydrate<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;

    /// <summary>
    ///     Hydrates the aggregate root from the Event Store
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="position">The position of the first event to load into the aggregate.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> HydrateFromPosition<T>(
        T aggregate,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate;

    /// <summary>
    ///     Hydrates the aggregate root from the Event Store
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="position">The position of the last event to load into the aggregate.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> HydrateUntilPosition<T>(
        T aggregate,
        long position,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate;

    /// <summary>
    ///     Hydrates the aggregate root from the Event Store
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="timestamp">The timestamp of the first event to load into the aggregate.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> HydrateFromTimestamp<T>(
        T aggregate,
        DateTime timestamp,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate;

    /// <summary>
    ///     Hydrates the aggregate root from the Event Store
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="timestamp">The timestamp of the last event to load into the aggregate.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<T> HydrateUntilTimestamp<T>(
        T aggregate,
        DateTime timestamp,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate;

    /// <summary>
    ///     Subscribes asynchronously to a default stream for the aggregate.
    /// </summary>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    /// ///
    /// <exception cref="SubscriptionFailed"></exception>
    Task SubscribeTo<T>(CancellationToken cancellationToken = default)
        where T : Aggregate;
}
