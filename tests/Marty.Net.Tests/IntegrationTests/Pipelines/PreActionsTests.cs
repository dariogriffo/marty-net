namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using Handlers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class PreActionsTests
{
    [Fact]
    public async Task When_Actions_Are_Registered_They_Are_Executed()
    {
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
            .AddSingleton(counter)
            .AddSingleton(beforeCounter)
            .AddSingleton(afterCounter);
        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderAbandoned @event = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        await eventStore.SubscribeToStream(streamName, SubscriptionPosition.Start);
        await eventStore.Save(streamName, @event, default);
        await Task.Delay(TimeSpan.FromSeconds(1));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }
}
