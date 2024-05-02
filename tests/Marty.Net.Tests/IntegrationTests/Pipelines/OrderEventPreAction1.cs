namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using System.Threading;
using System.Threading.Tasks;
using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

public class OrderEventPreAction1(ICounter counter) : IPreProcessor<OrderRefundRequested>
{
    public Task Execute(
        OrderRefundRequested @event,
        IConsumerContext context,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        return Task.CompletedTask;
    }
}
