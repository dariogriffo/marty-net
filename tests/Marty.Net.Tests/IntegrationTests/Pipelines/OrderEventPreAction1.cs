namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using System.Threading;
using System.Threading.Tasks;
using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

public class OrderEventPreAction1 : IPreProcessor<OrderRefundRequested>
{
    private readonly ICounter _counter;

    public OrderEventPreAction1(ICounter counter)
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
