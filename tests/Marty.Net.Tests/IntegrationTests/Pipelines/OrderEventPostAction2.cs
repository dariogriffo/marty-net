namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using System.Threading;
using System.Threading.Tasks;
using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

public class OrderEventPostAction2(ICounter counter) : IPostProcessor<OrderRefundRequested>
{
    public Task<OperationResult> Execute(
        OrderRefundRequested @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        return Task.FromResult(result);
    }
}
