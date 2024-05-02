namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using Contracts;
using Events.Orders;
using Handlers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class PostActionsTests
{
    [Fact]
    public async Task When_Actions_Are_Registered_They_Are_Executed()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

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
                        .AddPostProcessor<OrderEventPostAction1>()
                        .AddPostProcessor<OrderEventPostAction2>()
                        .AddPostProcessor<OrderEventPostAction3>()
                        .AddPostProcessor<OrderEventPostAction4>()
                        .AddPreProcessor<OrderEventPreAction1>()
                        .AddPreProcessor<OrderEventPreAction2>()
                        .AddPreProcessor<OrderEventPreAction3>()
                        .AddPreProcessor<OrderEventPreAction4>()
                        .AddBehavior<OrderPipelineBehaviorPipelineBehaviorAction1>()
                        .AddBehavior<OrderPipelineBehaviorPipelineBehaviorAction2>();
                }
            )
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderDelivered @event = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        CancellationToken cancellationToken = default;
        await eventStore.SubscribeToStream(
            streamName,
            SubscriptionPosition.Start,
            cancellationToken
        );
        await eventStore.Save(streamName, @event, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }
}
