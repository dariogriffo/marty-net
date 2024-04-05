namespace Marty.Net.Contracts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;

/// <summary>
///     The interface that gives access to write to https://github.com/EventStore/EventStore
///     <see cref="ConnectionFailed" /> is thrown on disconnection or failure to connect
/// </summary>
public interface IConnectionStrategy
{
    /// <summary>
    ///     The strategy of execution when a connection is broken.
    /// </summary>
    /// <param name="func">A Marty.Net internal function that is called with your strategy.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
    /// <returns>.</returns>
    Task Execute(Func<CancellationToken, Task> func, CancellationToken cancellationToken = default);

    /// <summary>
    ///     The strategy of execution when a connection is broken.
    /// </summary>
    /// <param name="func">A Marty.Net internal function that is called with your strategy.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
    /// <returns>.</returns>
    Task<T> Execute<T>(
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default
    );
}
