namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class PreAppendTouch : IPreAppendEventAction<OrderAbandoned>
{
    private readonly IBeforePublishCounter _counter;

    public PreAppendTouch(IBeforePublishCounter counter)
    {
        _counter = counter;
    }

    public Task Execute(OrderAbandoned @event, CancellationToken cancellationToken = default)
    {
        _counter.Touch();
        IDictionary<string, string> metadata = new Dictionary<string, string>();
        if (@event.Metadata is not null)
        {
            foreach ((string key, string value) in @event.Metadata)
            {
                metadata.Add(key, value);
            }
        }

        metadata.Add("test", "test");
        @event.Metadata = new ReadOnlyDictionary<string, string>(metadata);

        return Task.CompletedTask;
    }
}
