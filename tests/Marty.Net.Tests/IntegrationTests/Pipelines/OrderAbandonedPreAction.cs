namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class OrderAbandonedPreAction : IPreProcessor<OrderAbandoned>
{
    public Task Execute(
        OrderAbandoned @event,
        IConsumerContext context,
        CancellationToken cancellationToken = default
    )
    {
        Dictionary<string, string>? metadata = @event.Metadata?.ToDictionary(
            x => x.Key,
            x => x.Value
        );
        metadata?.Remove("test");
        @event.Metadata = metadata?.ToFrozenDictionary();
        return Task.CompletedTask;
    }
}
