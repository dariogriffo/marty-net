namespace Marty.Net.Tests.UnitTests.Stores;

using Contracts;
using Events.Orders;
using Internal;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

public class EventStoreTests
{
    [Fact]
    public async Task Save_Event_Proxies_The_Event_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        string streamName = $"order-{orderId.ToString()}";

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Save(streamName, e1);

        Mock.Get(write).Verify(x => x.Save(streamName, e1, default), Times.Once);
    }

    [Fact]
    public async Task Save_Events_Proxies_The_Events_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderRefundRequested e3 = new() { OrderId = orderId };
        IEvent[] events = [e2, e3];

        string streamName = $"order-{orderId.ToString()}";

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Save(streamName, e1);

        await sut.Append(streamName, events);

        Mock.Get(write).Verify(x => x.Append(streamName, events, default), Times.Once);
    }

    [Fact]
    public async Task Append_Event_Proxies_The_Event_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };

        string streamName = $"order-{orderId.ToString()}";

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Save(streamName, e1);
        await sut.Append(streamName, e2);

        Mock.Get(write).Verify(x => x.Append(streamName, e2, default), Times.Once);
    }

    [Fact]
    public async Task Append_Events_Proxies_The_Events_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderRefundRequested e3 = new() { OrderId = orderId };
        IEvent[] events = [e2, e3];

        string streamName = $"order-{orderId.ToString()}";

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Append(streamName, e1);
        await sut.Append(streamName, events);

        Mock.Get(write).Verify(x => x.Append(streamName, events, default), Times.Once);
    }

    [Fact]
    public async Task AppendWithExpectedStreamVersion_Event_Proxies_The_Event_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };

        string streamName = $"order-{orderId.ToString()}";

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Save(streamName, e1);
        await sut.Append(e2, 0);

        Mock.Get(write).Verify(x => x.Append(e2, 0, default), Times.Once);
    }

    [Fact]
    public async Task AppendWithStreamNameAndExpectedStreamVersion_Event_Proxies_The_Event_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };

        string streamName = $"order-{orderId.ToString()}";

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Save(streamName, e1);
        await sut.Append(streamName, e2, 0);

        Mock.Get(write).Verify(x => x.Append(streamName, e2, 0, default), Times.Once);
    }

    [Fact]
    public async Task AppendWithExpectedStreamVersion_Events_Proxies_The_Events_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };
        OrderCancelled e2 = new() { OrderId = orderId };
        OrderRefundRequested e3 = new() { OrderId = orderId };
        IEvent[] events = [e2, e3];

        string streamName = $"order-{orderId.ToString()}";

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Save(streamName, e1);
        await sut.Append(streamName, events, 0);

        Mock.Get(write).Verify(x => x.Append(streamName, events, 0, default), Times.Once);
    }

    [Fact]
    public async Task Append_Event_With_No_StreamName_Proxies_The_Event_Correctly()
    {
        IWriteEventStore write = Mock.Of<IWriteEventStore>();
        IReadEventStore _ = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new() { OrderId = orderId };

        IEventStore sut = new EventStore(write, _, _1, _2);

        await sut.Append(e1);

        Mock.Get(write).Verify(x => x.Append(e1, default), Times.Once);
    }

    [Fact]
    public async Task ReadStreamUntilPosition_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore read = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        string streamName = Guid.NewGuid().ToString();
        long position = Random.Shared.Next();
        IEventStore sut = new EventStore(_, read, _1, _2);

        await sut.ReadStreamUntilPosition(streamName, position);

        Mock.Get(read)
            .Verify(x => x.ReadStreamUntilPosition(streamName, position, default), Times.Once);
    }

    [Fact]
    public async Task ReadStreamUntilTimestamp_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore read = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        string streamName = Guid.NewGuid().ToString();
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;
        IEventStore sut = new EventStore(_, read, _1, _2);

        await sut.ReadStreamUntilTimestamp(streamName, timestamp);

        Mock.Get(read)
            .Verify(x => x.ReadStreamUntilTimestamp(streamName, timestamp, default), Times.Once);
    }

    [Fact]
    public async Task ReadStreamFromTimestamp_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore read = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        string streamName = Guid.NewGuid().ToString();
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;
        IEventStore sut = new EventStore(_, read, _1, _2);

        await sut.ReadStreamFromTimestamp(streamName, timestamp, default);

        Mock.Get(read)
            .Verify(x => x.ReadStreamFromTimestamp(streamName, timestamp, default), Times.Once);
    }

    [Fact]
    public async Task ReadStream_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore read = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _1 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        string streamName = Guid.NewGuid().ToString();
        IEventStore sut = new EventStore(_, read, _1, _2);

        await sut.ReadStream(streamName);

        Mock.Get(read).Verify(x => x.ReadStream(streamName, default), Times.Once);
    }

    [Fact]
    public async Task SubscribeToStream_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore _1 = Mock.Of<IReadEventStore>();
        IPersistentSubscriber subscriber = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        string streamName = Guid.NewGuid().ToString();
        IEventStore sut = new EventStore(_, _1, subscriber, _2);

        await sut.SubscribeToStream(streamName, SubscriptionPosition.Start);

        Mock.Get(subscriber)
            .Verify(
                x => x.SubscribeToStream(streamName, SubscriptionPosition.Start, default),
                Times.Once
            );
    }

    [Theory]
    [InlineData(SubscriptionPosition.Start)]
    [InlineData(SubscriptionPosition.End)]
    public async Task SubscribeToAll_Proxies_The_Call_Correctly(SubscriptionPosition position)
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore _1 = Mock.Of<IReadEventStore>();
        IPersistentSubscriber subscriber = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        IEventStore sut = new EventStore(_, _1, subscriber, _2);

        await sut.SubscribeToAll(position);

        Mock.Get(subscriber).Verify(x => x.SubscribeToAll(position, default), Times.Once);
    }

    [Fact]
    public async Task SubscribeToAllFromTheStart_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore _1 = Mock.Of<IReadEventStore>();
        IPersistentSubscriber subscriber = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        IEventStore sut = new EventStore(_, _1, subscriber, _2);

        await sut.SubscribeToAllFromTheStart();

        Mock.Get(subscriber)
            .Verify(x => x.SubscribeToAll(SubscriptionPosition.Start, default), Times.Once);
    }

    [Fact]
    public async Task SubscribeToAllFromTheEnd_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore _1 = Mock.Of<IReadEventStore>();
        IPersistentSubscriber subscriber = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        IEventStore sut = new EventStore(_, _1, subscriber, _2);

        await sut.SubscribeToAllFromTheEnd();

        Mock.Get(subscriber)
            .Verify(x => x.SubscribeToAll(SubscriptionPosition.End, default), Times.Once);
    }

    [Fact]
    public async Task SubscribeToStreamFromTheStart_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore _1 = Mock.Of<IReadEventStore>();
        IPersistentSubscriber subscriber = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        IEventStore sut = new EventStore(_, _1, subscriber, _2);
        Guid orderId = Guid.NewGuid();
        string streamName = $"order-{orderId.ToString()}";

        await sut.SubscribeToStreamFromTheStart(streamName);

        Mock.Get(subscriber)
            .Verify(
                x => x.SubscribeToStream(streamName, SubscriptionPosition.Start, default),
                Times.Once
            );
    }

    [Fact]
    public async Task SubscribeToStreamFromTheEnd_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore _1 = Mock.Of<IReadEventStore>();
        IPersistentSubscriber subscriber = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider _2 = Mock.Of<IConnectionProvider>();
        IEventStore sut = new EventStore(_, _1, subscriber, _2);
        Guid orderId = Guid.NewGuid();
        string streamName = $"order-{orderId.ToString()}";

        await sut.SubscribeToStreamFromTheEnd(streamName);

        Mock.Get(subscriber)
            .Verify(
                x => x.SubscribeToStream(streamName, SubscriptionPosition.End, default),
                Times.Once
            );
    }

    [Fact]
    public async Task StopConnections_Proxies_The_Call_Correctly()
    {
        IWriteEventStore _ = Mock.Of<IWriteEventStore>();
        IReadEventStore _1 = Mock.Of<IReadEventStore>();
        IPersistentSubscriber _2 = Mock.Of<IPersistentSubscriber>();
        IConnectionProvider connectionProvider = Mock.Of<IConnectionProvider>();
        IEventStore sut = new EventStore(_, _1, _2, connectionProvider);

        await sut.StopConnections();

        Mock.Get(connectionProvider).Verify(x => x.StopConnections(), Times.Once);
    }
}
