namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class PostAppendTouch : IPostAppendEventAction<OrderAbandoned>
{
    private readonly IAfterPublishCounter _counter;

    public PostAppendTouch(IAfterPublishCounter counter)
    {
        _counter = counter;
    }

    public Task Execute(
        WriteEnvelope<OrderAbandoned> envelope,
        CancellationToken cancellationToken = default
    )
    {
        _counter.Touch();
        return Task.CompletedTask;
    }
}
