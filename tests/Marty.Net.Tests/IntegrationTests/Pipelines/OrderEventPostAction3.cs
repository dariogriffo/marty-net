namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders;
using System.Threading;
using System.Threading.Tasks;

public class OrderEventPostAction3 : IPostProcessor<OrderDelivered>
{
    private readonly ICounter _counter;

    public OrderEventPostAction3(ICounter counter)
    {
        _counter = counter;
    }

    public Task<OperationResult> Execute(
        OrderDelivered @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.FromResult(result);
    }
}
