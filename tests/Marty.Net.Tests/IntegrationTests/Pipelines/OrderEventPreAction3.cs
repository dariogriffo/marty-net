namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders;
using System.Threading;
using System.Threading.Tasks;

public class OrderEventPreAction3(ICounter counter) : IPreProcessor<OrderDelivered>
{
    public Task Execute(
        OrderDelivered @event,
        IConsumerContext context,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        return Task.CompletedTask;
    }
}
