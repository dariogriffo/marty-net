namespace Marty.Net.Internal.Wrappers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class PostAppendEventActionWrapper
{
    internal abstract Task Execute(object @event, CancellationToken cancellationToken = default);
}

internal class PostAppendEventActionWrapper<T> : PostAppendEventActionWrapper
    where T : IEvent
{
    private readonly IPostAppendEventAction<T> _action;

    public PostAppendEventActionWrapper(IPostAppendEventAction<T> action)
    {
        _action = action;
    }

    internal override Task Execute(object @event, CancellationToken cancellationToken = default) =>
        _action.Execute((WriteEnvelope<T>)@event, cancellationToken);
}
