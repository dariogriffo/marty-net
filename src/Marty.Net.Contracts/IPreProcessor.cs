namespace Marty.Net.Contracts;

using Internal;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     Action to be executed before the <see cref="IEventHandler{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
[PreProcessorEvent]
public interface IPreProcessor<in T>
    where T : IEvent
{
    /// <summary>
    ///     Implement this to execute actions before <see cref="OperationResult" />.
    ///     Returns an <see cref="OperationResult" /> specifying what the event store subscription should do with the event
    ///     after being processed.
    /// </summary>
    /// <param name="event">The event that appeared on the persistent subscription.</param>
    /// <param name="context">The context of the event <see cref="IConsumerContext" />.</param>
    /// <param name="cancellationToken">The <see cref="System.Threading" />.</param>
    /// <returns>The task with an <see cref="IEventHandler{T}" /> to be awaited.</returns>
    Task Execute(T @event, IConsumerContext context, CancellationToken cancellationToken = default);
}
