namespace Marty.Net.Tests.UnitTests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ServiceCollectionExtensionTests
{
    [Fact]
    public async Task With_No_Parameters_We_Can_Run()
    {
        CancellationToken cancellationToken = default;
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings())
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderRefundRequested e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId}";
        await eventStore.SubscribeToStream(
            streamName,
            SubscriptionPosition.Start,
            cancellationToken
        );
        await eventStore.Save(streamName, e1, cancellationToken);
        await eventStore.Append(streamName, e2, cancellationToken);
        await eventStore.Append(streamName, e3, cancellationToken);
        List<IEvent> events = await readEventStore.ReadStream(streamName, cancellationToken);
        events.Count.Should().Be(3);
    }
}
