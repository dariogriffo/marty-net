namespace Marty.Net.Contracts;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;

/// <summary>
///     The interface that gives read access to https://github.com/EventStore/EventStore
/// </summary>
public interface IReadEventStore
{
    /// <summary>
    ///     Reads all the events from the stream.
    ///     If the stream doesn't exist throw <see cref="StreamNotFound" />
    ///     If the there is a connection exception and the retry is exhausted throw <see cref="ConnectionFailed" />
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStream(string streamName, CancellationToken cancellationToken = default);

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
    ///     Reads all the events from the stream starting at the timestamp
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="timestamp">The timestamp to start reading.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStreamFromTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Reads all the events from the stream until the Id of the lastEventToRead
    ///     matches one in the stream or the end of the stream is reached
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="position">The position to start read until &lt; <see cref="ulong.MaxValue" />.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStreamUntilPosition(
        string streamName,
        long position,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Reads all the events from the stream that their Timestamp is lower or equal than timestamp
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="timestamp">The timestamp in UTC included to load events.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken" />.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata.</returns>
    Task<List<IEvent>> ReadStreamUntilTimestamp(
        string streamName,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken = default
    );
}
