namespace Marty.Net.Internal;

using Contracts;
using Contracts.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class NoReconnectionStrategy : IConnectionStrategy
{
    public Task Execute(
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken = default
    )
    {
        return func(cancellationToken);
    }

    public Task<T> Execute<T>(
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default
    )
    {
        return func(cancellationToken);
    }
}

internal sealed class BasicReconnectionStrategy : IConnectionStrategy
{
    private int _retry;

    public async Task Execute(
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken = default
    )
    {
        Exception? last = null;
        while (_retry < 4)
        {
            try
            {
                await func(cancellationToken);
                _retry = 0;
                return;
            }
            catch (ConnectionFailed ex)
            {
                last = ex;
                ++_retry;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        if (last is not null)
        {
            throw last;
        }
    }

    public async Task<T> Execute<T>(
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default
    )
    {
        T result = default!;
        Exception? last = null;
        while (_retry < 4)
        {
            try
            {
                result = await func(cancellationToken);
                _retry = 0;
                return result;
            }
            catch (ConnectionFailed ex)
            {
                last = ex;
                ++_retry;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        if (last is not null)
        {
            throw last;
        }

        return result;
    }
}
