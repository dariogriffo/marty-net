namespace Marty.Net.Internal.Wrappers;

using Contracts;
using System.Threading;
using System.Threading.Tasks;

internal abstract class PreAppendEventActionWrapper
{
    internal abstract Task Execute(object @event, CancellationToken cancellationToken = default);
}

internal class PreAppendEventActionWrapper<T>(IPreAppendEventAction<T> action)
    : PreAppendEventActionWrapper
    where T : IEvent
{
    internal override Task Execute(object @event, CancellationToken cancellationToken = default) =>
        action.Execute((T)@event, cancellationToken);
}
