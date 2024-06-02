namespace Marty.Net.Contracts;

using System.Threading;
using System.Threading.Tasks;
using Internal;

/// <summary>
///     Action to be executed before publishing an <see cref="IEvent"/>
/// </summary>
/// <typeparam name="T"></typeparam>
[PreAppendEvent]
public interface IPreAppendEventAction<T>
    where T : IEvent
{
    /// <summary>
    /// Action to be executed before publishing an <see cref="IEvent"/>
    /// </summary>
    /// <param name="envelope">The event envelope</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task Execute(WriteEnvelope<T> envelope, CancellationToken cancellationToken = default);
}
