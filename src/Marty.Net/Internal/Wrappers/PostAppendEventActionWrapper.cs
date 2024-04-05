namespace Marty.Net.Internal.Wrappers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class PostAppendEventActionWrapper
{
    internal abstract Task Execute(object @event, CancellationToken cancellationToken = default);
}

internal class PostAppendEventActionWrapper<T>(IPostAppendEventAction<T> action)
    : PostAppendEventActionWrapper
    where T : IEvent
{
    internal override Task Execute(object @event, CancellationToken cancellationToken = default) =>
        action.Execute((T)@event, cancellationToken);
}
