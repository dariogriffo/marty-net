namespace Marty.Net.Tests.IntegrationTests;

using Contracts;
using Events.Orders;
using FluentAssertions;
using Handlers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ConsumerContextTests
{
    [Fact]
    public async Task StreamNameIsCorrectlySet()
    {
        CancellationToken cancellationToken = default;

        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(
                sp => sp.GetEventStoreSettings(),
                c =>
                {
                    c.AssembliesToScanForHandlers = [typeof(OrderEventHandlerHandler).Assembly];
                }
            )
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderRefundRequested e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";

        await eventStore.SubscribeToStream(
            streamName,
            SubscriptionPosition.Start,
            cancellationToken
        );
        await eventStore.Save(streamName, e1, cancellationToken);
        await eventStore.Append(streamName, e2, cancellationToken);
        await eventStore.Append(streamName, e3, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        IStreamNames streamNames = provider.GetRequiredService<IStreamNames>();
        streamNames.Count().Should().Be(3);
        streamNames.Streams.Distinct().Should().BeEquivalentTo([streamName]);
    }
}
