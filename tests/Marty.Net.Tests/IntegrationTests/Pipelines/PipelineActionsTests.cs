namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System;
using System.Threading.Tasks;
using Contracts;
using Events.Orders.v2;
using Handlers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class PipelineActionsTests
{
    [Fact]
    public async Task When_Actions_Are_Registered_They_Are_Executed()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
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
                        .AddBehavior<OrderPipelineBehaviorPipelineBehaviorAction1>()
                        .AddBehavior<OrderPipelineBehaviorPipelineBehaviorAction2>();
                }
            );

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        string streamName = $"order-{orderId}";
        OrderEventCancelled @event = new() { OrderId = orderId, Reason = "No reason" };
        await eventStore.SubscribeToStream(streamName, SubscriptionPosition.Start);
        await eventStore.Save(streamName, @event, default);
        await Task.Delay(TimeSpan.FromSeconds(2));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(5));
    }
}
