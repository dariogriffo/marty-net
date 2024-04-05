namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

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
