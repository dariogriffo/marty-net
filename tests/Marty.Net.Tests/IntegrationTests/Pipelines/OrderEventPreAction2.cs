namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

public class OrderEventPreAction2 : IPreProcessor<OrderRefundRequested>
{
    private readonly ICounter _counter;

    public OrderEventPreAction2(ICounter counter)
    {
        _counter = counter;
    }

    public Task Execute(
        OrderRefundRequested @event,
        IConsumerContext context,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.CompletedTask;
    }
}
