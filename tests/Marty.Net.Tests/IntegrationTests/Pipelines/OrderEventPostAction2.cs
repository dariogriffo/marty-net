namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using System.Threading;
using System.Threading.Tasks;
using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

public class OrderEventPostAction2 : IPostProcessor<OrderRefundRequested>
{
    private readonly ICounter _counter;

    public OrderEventPostAction2(ICounter counter)
    {
        _counter = counter;
    }

    public Task<OperationResult> Execute(
        OrderRefundRequested @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.FromResult(result);
    }
}
