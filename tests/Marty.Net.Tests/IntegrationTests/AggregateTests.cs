namespace Marty.Net.Tests.IntegrationTests;

using System;
using System.Threading.Tasks;
using Aggregates;
using Contracts;
using Contracts.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Net.Aggregates.Contracts;
using Xunit;

public class AggregateTests
{
    [Fact]
    public async Task Saved_Aggregate_Is_Correctly_Loaded()
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
        await store.Create(user);
        User user1 = await store.GetAggregateById<User>(user.Id, default);
        user1.Sum.Should().Be(11);
        user1.Status.Should().Be(User.UserStatus.Inactive);
    }

    [Fact]
    public async Task Create_Fails_On_Existing_Stream()
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
        User user1 = User.Create(userId);
        await store.Create(user);
        Func<Task> act = async () => await store.Create(user1);
        await act.Should().ThrowAsync<StreamAlreadyExists>();
    }

    [Fact]
    public async Task Save_And_Load_WorkAsExpected()
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
        User user1 = User.WithId(userId);
        await user1.Load(store);
        user1.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task Update_WorkAsExpected()
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
        User user1 = User.WithId(userId);
        await user1.Load(store);
        user1.Activate();
        await user1.Update(store);

        User user2 = User.WithId(userId);
        await user2.Load(store);
        user2.Should().BeEquivalentTo(user1);

        user2.Delete();
        await user2.Update(store);

        User user3 = User.WithId(userId);
        await user3.Load(store);
        user3.Should().BeEquivalentTo(user2);
    }
}
