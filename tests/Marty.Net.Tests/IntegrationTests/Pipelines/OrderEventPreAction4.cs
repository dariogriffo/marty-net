namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders;
using System.Threading;
using System.Threading.Tasks;

public class OrderEventPreAction4(ICounter counter) : IPreProcessor<OrderAbandoned>
{
    public Task Execute(
        OrderAbandoned @event,
        IConsumerContext context,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        return Task.CompletedTask;
    }
}
