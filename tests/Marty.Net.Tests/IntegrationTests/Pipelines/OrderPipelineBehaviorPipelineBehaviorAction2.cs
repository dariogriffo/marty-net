namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders.v2;
using System;
using System.Threading;
using System.Threading.Tasks;

public class OrderPipelineBehaviorPipelineBehaviorAction2 : IPipelineBehavior<OrderEventCancelled>
{
    private readonly ICounter _counter;

    public OrderPipelineBehaviorPipelineBehaviorAction2(ICounter counter)
    {
        _counter = counter;
    }

    public async Task<OperationResult> Execute(
        OrderEventCancelled @event,
        IConsumerContext context,
        Func<OrderEventCancelled, IConsumerContext, CancellationToken, Task<OperationResult>> next,
        CancellationToken cancellationToken = default
    )
    {
        _counter.Touch();
        OperationResult result = await next(@event, context, cancellationToken);
        _counter.Touch();
        return result;
    }
}
