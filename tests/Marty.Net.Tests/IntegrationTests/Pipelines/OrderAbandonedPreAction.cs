namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class OrderAbandonedPreAction : IPreProcessor<OrderAbandoned>
{
    public Task Execute(
        OrderAbandoned @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        Dictionary<string, string>? metadata = context.Metadata?.ToDictionary(
            x => x.Key,
            x => x.Value
        );
        metadata?.Remove("test");
        return Task.CompletedTask;
    }
}
