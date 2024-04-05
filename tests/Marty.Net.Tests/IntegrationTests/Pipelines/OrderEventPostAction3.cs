namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class OrderEventPostAction3(ICounter counter) : IPostProcessor<OrderDelivered>
{
    public Task<OperationResult> Execute(
        OrderDelivered @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        return Task.FromResult(result);
    }
}
