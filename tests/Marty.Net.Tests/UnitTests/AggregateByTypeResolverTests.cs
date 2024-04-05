namespace Marty.Net.Tests.UnitTests;

using System;
using Aggregates;
using Contracts.Internal;
using FluentAssertions;
using Xunit;

public class AggregateByTypeResolverTests
{
    [Fact]
    public void StreamForAggregateWithObject_WithValidId_ReturnsValidStreamName()
    {
        User aggregate = User.Create("UserId");
        AggregateByTypeResolver sut = new();
        string result = sut.StreamForAggregate(aggregate);
        result.Should().Be("user-UserId");
    }

    [Fact]
    public void StreamForAggregateWithGeneric_WithValidId_ReturnsValidStreamName()
    {
        User aggregate = User.Create("UserId");
        AggregateByTypeResolver sut = new();
        string result = sut.StreamForAggregate<User>(aggregate.Id);
        result.Should().Be("user-UserId");
    }

    [Fact]
    public void AggregateIdForStream_WithValidStreamName_ReturnsTheAggregateId()
    {
        AggregateByTypeResolver sut = new();
        string id = Guid.NewGuid().ToString();
        string result = sut.AggregateIdForStream($"user-{id}");
        result.Should().Be(id);
    }
}
