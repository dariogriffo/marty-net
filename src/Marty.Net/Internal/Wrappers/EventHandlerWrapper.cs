namespace Marty.Net.Internal.Wrappers;

using Contracts;
using System.Threading;
using System.Threading.Tasks;

internal abstract class EventHandlerWrapper
{
    internal abstract Task<OperationResult> Handle(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    );
}

internal class EventHandlerWrapper<T> : EventHandlerWrapper
    where T : IEvent
{
    private readonly IEventHandler<T> _handler;

    public EventHandlerWrapper(IEventHandler<T> handler)
    {
        _handler = handler;
    }

    internal override Task<OperationResult> Handle(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        return _handler.Handle((T)@event, context, cancellationToken);
    }
}
