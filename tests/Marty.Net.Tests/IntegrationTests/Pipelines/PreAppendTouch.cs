namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System.Collections.Generic;
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

    public Task Execute(
        WriteEnvelope<OrderAbandoned> envelope,
        CancellationToken cancellationToken = default
    )
    {
        _counter.Touch();
        IDictionary<string, string> metadata = new Dictionary<string, string>();
        if (envelope.Metadata is not null)
        {
            foreach ((string key, string value) in envelope.Metadata)
            {
                metadata.Add(key, value);
            }
        }

        metadata.Add("test", "test");

        return Task.CompletedTask;
    }
}
