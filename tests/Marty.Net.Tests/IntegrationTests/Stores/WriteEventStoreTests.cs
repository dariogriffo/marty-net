namespace Marty.Net.Tests.IntegrationTests.Stores;

using System;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using Events.Orders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class WriteEventStoreTests
{
    [Fact]
    public async Task Append_Events_With_Unexpected_Version_Throws()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] batchOne = [e1, e2];
        await eventStore.Save(streamName, batchOne, default);
        IEvent[] batchTwo = [e3];
        Func<Task> act = async () => await eventStore.Append(streamName, batchTwo, 2, default);

        await act.Should()
            .ThrowAsync<MismatchExpectedVersion>()
            .WithMessage($"Stream {streamName} expected to have version 2 but found 1");
    }

    [Fact]
    public async Task Append_Event_With_StreamName_With_Unexpected_Version_Throws()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        Guid orderId = Guid.NewGuid();
        string streamName = $"order-{orderId.ToString()}";

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());
        TestStreamResolver streamResolver = new(streamName);
        services.AddSingleton<IEventsStreamResolver>(streamResolver);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();

        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderCancelled e3 = new() { OrderId = orderId };

        IEvent[] batchOne = [e1, e2];
        await eventStore.Save(streamName, batchOne, default);
        Func<Task> act = async () => await eventStore.Append(e3, 2, default);

        await act.Should()
            .ThrowAsync<MismatchExpectedVersion>()
            .WithMessage($"Stream {streamName} expected to have version 2 but found 1");
    }

    [Fact]
    public async Task Append_Event_With_Unexpected_Version_Throws()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings());

        await using ServiceProvider provider = services.BuildServiceProvider();
        IWriteEventStore eventStore = provider.GetRequiredService<IWriteEventStore>();

        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderCancelled e3 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";
        IEvent[] batchOne = [e1, e2];
        await eventStore.Save(streamName, batchOne, default);
        Func<Task> act = async () => await eventStore.Append(streamName, e3, 2, default);

        await act.Should()
            .ThrowAsync<MismatchExpectedVersion>()
            .WithMessage($"Stream {streamName} expected to have version 2 but found 1");
    }
}
