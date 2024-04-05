namespace Marty.Net.Tests.IntegrationTests.Stores;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using Events.Orders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ReadEventStoreTests
{
    [Fact]
    public async Task ReadStream_Test()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] writeEvents = [e1, e2, e3];
        await eventStore.Save(streamName, writeEvents, default);
        List<IEvent> events = await readEventStore.ReadStream(streamName, default);
        events.Count.Should().Be(3);
        events[0].Should().BeEquivalentTo(e1);
        events[1].Should().BeEquivalentTo(e2);
        events[2].Should().BeEquivalentTo(e3);
    }

    [Fact]
    public async Task ReadStreamUntilEvent_Test()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] writeEvents = [e1, e2, e3];
        await eventStore.Save(streamName, writeEvents, default);
        int position = 1;
        List<IEvent> events = await readEventStore.ReadStreamUntilPosition(
            streamName,
            position,
            default
        );
        events.Count.Should().Be(2);
        events[0].Should().BeEquivalentTo(e1);
        events[1].Should().BeEquivalentTo(e2);
    }

    [Fact]
    public async Task ReadStreamUntilTimestamp_Test()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        Thread.Sleep(TimeSpan.FromMilliseconds(1));
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] writeEvents = [e1, e2, e3];
        await eventStore.Save(streamName, writeEvents, default);
        List<IEvent> events = await readEventStore.ReadStreamUntilTimestamp(
            streamName,
            e2.Timestamp,
            default
        );
        events.Count.Should().Be(2);
        events[0].Should().BeEquivalentTo(e1);
        events[1].Should().BeEquivalentTo(e2);
    }

    [Fact]
    public async Task ReadStreamUntilPosition_Test()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        Thread.Sleep(TimeSpan.FromMilliseconds(1));
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] writeEvents = [e1, e2, e3];
        await eventStore.Save(streamName, writeEvents, default);
        List<IEvent> events = await readEventStore.ReadStreamUntilPosition(streamName, 1, default);
        events.Count.Should().Be(2);
        events[0].Should().BeEquivalentTo(e1);
        events[1].Should().BeEquivalentTo(e2);
    }

    [Fact]
    public async Task ReadStreamFromPosition_Test()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        Thread.Sleep(TimeSpan.FromMilliseconds(1));
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] writeEvents = [e1, e2, e3];
        await eventStore.Save(streamName, writeEvents, default);
        List<IEvent> events = await readEventStore.ReadStreamFromPosition(streamName, 1, default);
        events.Count.Should().Be(2);
        events[0].Should().BeEquivalentTo(e2);
        events[1].Should().BeEquivalentTo(e3);
    }

    [Fact]
    public async Task ReadStreamFromTimestamp_Test()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        Thread.Sleep(TimeSpan.FromMilliseconds(1));
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] writeEvents = [e1, e2, e3];
        await eventStore.Save(streamName, writeEvents, default);
        List<IEvent> events = await readEventStore.ReadStreamFromTimestamp(
            streamName,
            e2.Timestamp,
            default
        );
        events.Count.Should().Be(2);
        events[0].Should().BeEquivalentTo(e2);
        events[1].Should().BeEquivalentTo(e3);
    }

    [Fact]
    public async Task ReadStream_Throws_On_Non_Existing_Stream()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();

        string streamName = Guid.NewGuid().ToString();
        Func<Task> act = async () => await readEventStore.ReadStream(streamName, default);

        await act.Should()
            .ThrowAsync<StreamNotFound>()
            .WithMessage($"Stream {streamName} was not found");
    }
}
