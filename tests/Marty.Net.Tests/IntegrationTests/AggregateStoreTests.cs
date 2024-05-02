namespace Marty.Net.Tests.IntegrationTests;

using Aggregates;
using Contracts;
using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Net.Aggregates.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class AggregateStoreTests
{
    [Fact]
    public async Task HydrateUntilPosition_WorkAsExpected()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings())
            .AddMartyAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore store = provider.GetRequiredService<IAggregateStore>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        user.Deactivate();
        List<IEvent> events = user.UncommittedChanges.ToList();
        await user.Create(store);
        int position = events.Count - 2;
        User user1 = User.Create(userId);
        await store.HydrateUntilPosition(user1, position);
        user1.Status.Should().Be(User.UserStatus.Updated);
    }

    [Fact]
    public async Task HydrateFromPosition_When_Aggregate_Has_Events_Throws_Exception()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings())
            .AddMartyAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore store = provider.GetRequiredService<IAggregateStore>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        await user.Create(store);

        User user1 = await store.GetAggregateById<User>(userId);
        user.Deactivate();
        await user.Update(store);
        await store.HydrateFromPosition(user1, 2);
        user1.Should().BeEquivalentTo(user);
        user1.Deactivate();
        await user1.Update(store);

        User user2 = await store.GetAggregateById<User>(userId);
        user2.Deactivate();
        await user2.Update(store);

        User user3 = await store.GetAggregateById<User>(userId);

        Func<Task> act = async () => await store.Hydrate(user3);
        ExceptionAssertions<InvalidOperationException>? ex = await act.Should()
            .ThrowAsync<InvalidOperationException>();
        ex.WithMessage($"Aggregate with Id {user3.Id} cannot be hydrated since it has events");
    }

    [Fact]
    public async Task HydrateFromPosition_WorkAsExpected()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings())
            .AddMartyAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore store = provider.GetRequiredService<IAggregateStore>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        await user.Create(store);

        User user1 = await store.GetAggregateById<User>(userId);
        user.Deactivate();
        await user.Update(store);
        await store.HydrateFromPosition(user1, 2);
        user1.Should().BeEquivalentTo(user);
        user1.Deactivate();
        await user1.Update(store);

        User user2 = await store.GetAggregateById<User>(userId);
        user2.Deactivate();
        await user2.Update(store);

        User user3 = User.WithId(userId);
        await store.Hydrate(user3);
        user3.Deactivate();
        user3.Version.Should().Be(user2.Version + 1);
    }

    [Fact]
    public async Task GetAggregateFromStream_WorkAsExpected()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings())
            .AddMartyAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore store = provider.GetRequiredService<IAggregateStore>();
        IAggregateStreamResolver aggregateStreamResolver =
            provider.GetRequiredService<IAggregateStreamResolver>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        user.Deactivate();
        await user.Create(store);
        User user1 = await store.GetAggregateFromStream<User>(
            aggregateStreamResolver.StreamForAggregate(user)
        );
        user1.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetAggregateFromStream_With_LastEventToLoad_WorkAsExpected()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings())
            .AddMartyAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore store = provider.GetRequiredService<IAggregateStore>();
        IAggregateStreamResolver aggregateStreamResolver =
            provider.GetRequiredService<IAggregateStreamResolver>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        user.Deactivate();
        List<IEvent> events = user.UncommittedChanges.ToList();
        await user.Create(store);

        int position = events.Count - 2;
        User user1 = await store.GetAggregateFromStreamUntilPosition<User>(
            aggregateStreamResolver.StreamForAggregate(user),
            position
        );
        user1.Status.Should().Be(User.UserStatus.Updated);
    }

    [Fact]
    public async Task GetAggregateById_WorkAsExpected()
    {
        ServiceCollection services = [];
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddMarty(sp => sp.GetEventStoreSettings())
            .AddMartyAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore store = provider.GetRequiredService<IAggregateStore>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        user.Deactivate();
        await user.Create(store);
        User user1 = await store.GetAggregateById<User>(userId);
        user1.Should().BeEquivalentTo(user);
    }
}
