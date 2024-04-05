namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Grpc.Core;
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
