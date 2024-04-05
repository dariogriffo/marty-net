namespace Marty.Net.Contracts;

using System.Threading;
using System.Threading.Tasks;
using Exceptions;

/// <summary>
///     The interface that gives access to write to https://github.com/EventStore/EventStore
/// </summary>
public interface IWriteEventStore
{
    /// <summary>
    ///     Saves the event asynchronously. The streamName MUST NOT exist
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    /// <exception cref="StreamAlreadyExists"></exception>
    Task<long> Save<T>(string streamName, T @event, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    ///     Saves the event asynchronously. The streamName MUST NOT exist
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="events">The list of events to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    /// <exception cref="StreamAlreadyExists"></exception>
    Task<long> Save<T>(string streamName, T[] events, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    ///     Appends the event asynchronously.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<long> Append<T>(string streamName, T @event, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    ///     Appends the events asynchronously.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="events">The list of events to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<long> Append<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent;

    /// <summary>
    ///     Appends the event asynchronously.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<long> Append<T>(T @event, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    ///     Appends the event asynchronously.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="event">The event to be stored.</param>
    /// <param name="expectedStreamVersion">The version expected at the time of writing.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<long> Append<T>(
        string streamName,
        T @event,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent;

    /// <summary>
    ///     Appends the events asynchronously.
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="events">The list of events to be stored.</param>
    /// <param name="expectedStreamVersion">The version expected at the time of writing.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<long> Append<T>(
        string streamName,
        T[] events,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent;

    /// <summary>
    ///     Appends the event asynchronously.
    ///     Might throw <see cref="MismatchExpectedVersion" />
    /// </summary>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
    /// <param name="event">The event to be stored.</param>
    /// <param name="expectedStreamVersion">The version expected at the time of writing.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    Task<long> Append<T>(
        T @event,
        long expectedStreamVersion,
        CancellationToken cancellationToken = default
    )
        where T : IEvent;
}
