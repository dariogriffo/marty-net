namespace Marty.Net.Internal.Wrappers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class PreAppendEventActionWrapper
{
    internal abstract Task Execute(object @event, CancellationToken cancellationToken = default);
}

internal class PreAppendEventActionWrapper<T> : PreAppendEventActionWrapper
    where T : IEvent
{
    private readonly IPreAppendEventAction<T> _action;

    public PreAppendEventActionWrapper(IPreAppendEventAction<T> action)
    {
        _action = action;
    }

    internal override Task Execute(object @event, CancellationToken cancellationToken = default) =>
        _action.Execute((WriteEnvelope<T>)@event, cancellationToken);
}
