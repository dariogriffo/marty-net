namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders;
using System.Threading;
using System.Threading.Tasks;

public class OrderEventPostAction4 : IPostProcessor<OrderAbandoned>
{
    private readonly ICounter _counter;

    public OrderEventPostAction4(ICounter counter)
    {
        _counter = counter;
    }

    public Task<OperationResult> Execute(
        OrderAbandoned @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.FromResult(result);
    }
}
