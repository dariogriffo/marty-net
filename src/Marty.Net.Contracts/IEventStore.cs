namespace Marty.Net.Contracts;

using Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     The interface that gives access to https://github.com/EventStore/EventStore
/// </summary>
public interface IEventStore
{
    /// <summary>
    ///     Saves the event asynchronously. The streamName MUST NOT exist.
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
    ///     Saves the event asynchronously. The streamName MUST NOT exist.
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
    ///     Appends the event asynchronously. The streamName MUST exist.
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
    ///     Appends the events asynchronously. The streamName MUST exist.
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

    /// <summary>
    ///     Appends the event asynchronously.
    ///     Might throw <see cref="MismatchExpectedVersion" />
    /// </summary>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <typeparam name="T">
    ///     <see cref="IEvent" />
    /// </typeparam>
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
    ///     Appends the events asynchronously. The streamName MUST exist.
    ///     Might throw <see cref="MismatchExpectedVersion" />
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
    ///     Appends the event asynchronously. The streamName MUST exist.
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
    ///     Reads all the events from the stream starting at the position
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="position">The position to start reading from >=0.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStreamFromPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Reads all the events from the stream until position (inclusive)
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="position">The position to read until.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStreamUntilPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Reads all the events from the stream until the timestamp (included)
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="timestamp">The timestamp read until.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStreamUntilTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Reads all the events from the stream starting at the timestamps
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="timestamp">The timestamp read until.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStreamFromTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Reads all the events from the stream.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStream(string streamName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Subscribes asynchronously to a stream from the end.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    /// ///
    /// <exception cref="SubscriptionFailed"></exception>
    Task SubscribeToStreamFromTheEnd(
        string streamName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Subscribes asynchronously to a stream from the start.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    /// ///
    /// <exception cref="SubscriptionFailed"></exception>
    Task SubscribeToStreamFromTheStart(
        string streamName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Subscribes asynchronously to a stream.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="subscriptionPosition">The position to start the subscription from </param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task" /> to be awaited.</returns>
    /// ///
    /// <exception cref="SubscriptionFailed"></exception>
    Task SubscribeToStream(
        string streamName,
        SubscriptionPosition subscriptionPosition,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Subscribes asynchronously to the $all stream.
    /// </summary>
    /// <param name="subscriptionPosition">The position to start the subscription from </param>
    /// <param name="cancellationToken">The CancellationToken.</param>
    /// <exception cref="SubscriptionFailed"></exception>
    public Task SubscribeToAll(
        SubscriptionPosition subscriptionPosition,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Subscribes asynchronously to the $all stream from the start.
    /// </summary>
    /// <param name="cancellationToken">The CancellationToken.</param>
    /// <exception cref="SubscriptionFailed"></exception>
    public Task SubscribeToAllFromTheStart(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Subscribes asynchronously to the $all stream from the end.
    /// </summary>
    /// <param name="cancellationToken">The CancellationToken.</param>
    /// <exception cref="SubscriptionFailed"></exception>
    public Task SubscribeToAllFromTheEnd(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops all connections
    /// </summary>
    /// <returns></returns>
    public ValueTask StopConnections();
}
