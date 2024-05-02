namespace Marty.Net.Internal.Wrappers;

using Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

internal abstract class PipelineBehaviorWrapper
{
    protected internal Func<Task<OperationResult>> Next { get; set; } = null!;

    internal abstract Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    );
}

internal class PipelineBehaviorWrapper<T>(IPipelineBehavior<T> behavior) : PipelineBehaviorWrapper
    where T : IEvent
{
    internal override Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    ) => behavior.Execute((T)@event, context, Next, cancellationToken);
}
