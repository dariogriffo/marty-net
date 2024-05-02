namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders;
using System.Threading;
using System.Threading.Tasks;

public class PostAppendTouch : IPostAppendEventAction<OrderAbandoned>
{
    private readonly IAfterPublishCounter _counter;

    public PostAppendTouch(IAfterPublishCounter counter)
    {
        _counter = counter;
    }

    public Task Execute(OrderAbandoned @event, CancellationToken cancellationToken = default)
    {
        _counter.Touch();
        return Task.CompletedTask;
    }
}
