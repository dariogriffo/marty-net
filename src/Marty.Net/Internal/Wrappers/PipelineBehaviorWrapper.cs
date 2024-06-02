namespace Marty.Net.Internal.Wrappers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal abstract class PipelineBehaviorWrapper
{
    protected internal Func<
        object,
        IConsumerContext,
        CancellationToken,
        Task<OperationResult>
    > Next { get; set; } = null!;

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
    )
    {
        Func<T, IConsumerContext, CancellationToken, Task<OperationResult>>? next =
            Next as Func<T, IConsumerContext, CancellationToken, Task<OperationResult>>;
        return _behavior.Execute((T)@event, context, next!, cancellationToken);
    }
}
