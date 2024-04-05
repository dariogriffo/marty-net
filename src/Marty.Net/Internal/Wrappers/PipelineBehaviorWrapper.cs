namespace Marty.Net.Internal.Wrappers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class PipelineBehaviorWrapper
{
    internal abstract Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        Func<Task<OperationResult>> next,
        CancellationToken cancellationToken = default
    );
}

internal class PipelineBehaviorWrapper<T>(IPipelineBehavior<T> behavior) : PipelineBehaviorWrapper
    where T : IEvent
{
    internal override Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        Func<Task<OperationResult>> next,
        CancellationToken cancellationToken = default
    ) => behavior.Execute((T)@event, context, next, cancellationToken);
}
