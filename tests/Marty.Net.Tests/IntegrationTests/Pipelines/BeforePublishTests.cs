namespace Marty.Net.Tests.IntegrationTests.Pipelines;

using System;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using Handlers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class BeforePublishTests
{
    [Fact]
    public async Task When_BeforePublishActions_Are_Registered_They_Are_Executed()
    {
        ServiceCollection services = [];
        IBeforePublishCounter beforeCounter = Mock.Of<IBeforePublishCounter>();
        IAfterPublishCounter afterCounter = Mock.Of<IAfterPublishCounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(
                sp => sp.GetEventStoreSettings(),
                configuration =>
                    configuration
                        .AddPostAppendAction<PostAppendTouch>()
                        .AddPreAppendAction<PreAppendTouch>()
            )
            .AddSingleton(beforeCounter)
            .AddSingleton(afterCounter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderAbandoned @event = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        await eventStore.Save(streamName, @event, default);
        Mock<IBeforePublishCounter> mock = Mock.Get(beforeCounter);
        mock.Verify(x => x.Touch(), Times.Once);
    }

    [Fact]
    public async Task When_AfterPublishActions_Are_Registered_They_Are_Executed()
    {
        ServiceCollection services = [];
        IBeforePublishCounter beforeCounter = Mock.Of<IBeforePublishCounter>();
        IAfterPublishCounter afterCounter = Mock.Of<IAfterPublishCounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(
                sp => sp.GetEventStoreSettings(),
                configuration =>
                    configuration
                        .AddPostAppendAction<PostAppendTouch>()
                        .AddPreAppendAction<PreAppendTouch>()
            )
            .AddSingleton(beforeCounter)
            .AddSingleton(afterCounter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderAbandoned @event = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        await eventStore.Save(streamName, @event, default);
        Mock<IAfterPublishCounter> mock = Mock.Get(afterCounter);
        mock.Verify(x => x.Touch(), Times.Once);
    }
}
