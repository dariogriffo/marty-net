namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class OrderEventPreAction3 : IPreProcessor<OrderDelivered>
{
    private readonly ICounter _counter;

    public OrderEventPreAction3(ICounter counter)
    {
        _counter = counter;
    }

    public Task Execute(
        OrderDelivered @event,
        IConsumerContext context,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.CompletedTask;
    }
}
