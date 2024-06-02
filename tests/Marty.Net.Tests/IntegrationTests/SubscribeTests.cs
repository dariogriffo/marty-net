namespace Marty.Net.Tests.IntegrationTests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using Handlers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipelines;
using Xunit;

public class SubscribeTests
{
    [Fact]
    public async Task All_Events_Handled_Test()
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

        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }

    [Fact]
    public async Task Pre_And_Post_Append_Actions_Test()
    {
        CancellationToken cancellationToken = default;

        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        IBeforePublishCounter beforeCounter = Mock.Of<IBeforePublishCounter>();
        IAfterPublishCounter afterCounter = Mock.Of<IAfterPublishCounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(
                sp => sp.GetEventStoreSettings(),
                configuration =>
                {
                    configuration.AssembliesToScanForHandlers =
                    [
                        typeof(OrderEventHandlerHandler).Assembly
                    ];
                    configuration
                        .AddPreAppendAction<PreAppendTouch>()
                        .AddPostAppendAction<PostAppendTouch>();
                }
            )
            .AddSingleton(counter)
            .AddSingleton(beforeCounter)
            .AddSingleton(afterCounter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderAbandoned e1 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";

        await eventStore.SubscribeToStream(
            streamName,
            SubscriptionPosition.Start,
            cancellationToken
        );
        await eventStore.Save(streamName, e1, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        Mock<IBeforePublishCounter> before = Mock.Get(beforeCounter);
        before.Verify(x => x.Touch(), Times.Exactly(1));
        Mock<IAfterPublishCounter> after = Mock.Get(afterCounter);
        after.Verify(x => x.Touch(), Times.Exactly(1));
    }
}
