namespace Marty.Net.Internal.Wrappers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class EventHandlerWrapper
{
    internal abstract Task<OperationResult> Handle(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    );
}

internal class EventHandlerWrapper<T>(IEventHandler<T> handler) : EventHandlerWrapper
    where T : IEvent
{
    internal override Task<OperationResult> Handle(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        return handler.Handle((T)@event, context, cancellationToken);
    }
}