namespace Marty.Net.Contracts;

using Internal;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     Action to be executed after publishing an <see cref="IEvent"/>
/// </summary>
/// <typeparam name="T"></typeparam>
[AfterPublishEvent]
public interface IPostAppendEventAction<in T>
    where T : IEvent
{
    /// <summary>
    /// Action to be executed before publishing an <see cref="IEvent"/>
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(T @event, CancellationToken cancellationToken = default);
}
