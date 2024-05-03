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

internal class PipelineBehaviorWrapper<T> : PipelineBehaviorWrapper
    where T : IEvent
{
    private readonly IPipelineBehavior<T> _behavior;

    public PipelineBehaviorWrapper(IPipelineBehavior<T> behavior)
    {
        _behavior = behavior;
    }

    internal override Task<OperationResult> Execute(
        object @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    ) => _behavior.Execute((T)@event, context, Next, cancellationToken);
}
