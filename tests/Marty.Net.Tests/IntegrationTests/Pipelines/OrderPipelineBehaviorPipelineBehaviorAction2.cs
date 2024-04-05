namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders.v2;

public class OrderPipelineBehaviorPipelineBehaviorAction2(ICounter counter)
    : IPipelineBehavior<OrderEventCancelled>
{
    public async Task<OperationResult> Execute(
        OrderEventCancelled @event,
        IConsumerContext context,
        Func<Task<OperationResult>> next,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        OperationResult result = await next();
        counter.Touch();
        return result;
    }
}
