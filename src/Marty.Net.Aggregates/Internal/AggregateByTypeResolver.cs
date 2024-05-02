namespace Marty.Net.Contracts.Internal;

using Aggregates.Contracts;
using System;
using System.Collections.Concurrent;

internal sealed class AggregateByTypeResolver : IAggregateStreamResolver
{
    private readonly ConcurrentDictionary<Type, string> _cachedTypes = new();

    public string StreamForAggregate<T>(string aggregateId)
        where T : Aggregate
    {
        string prefix = _cachedTypes.GetOrAdd(typeof(T), _ => typeof(T).Name.ToSnakeCase());
        return $"{prefix}-{aggregateId}";
    }

    public string StreamForAggregate<T>(T aggregate)
        where T : Aggregate
    {
        Type type = aggregate.GetType();
        string prefix = _cachedTypes.GetOrAdd(type, _ => type.Name.ToSnakeCase());
        return $"{prefix}-{aggregate.Id}";
    }

    public string AggregateIdForStream(string streamName)
    {
        string oldValue = $"{streamName.Split('-')[0]}-";
        return streamName.Replace(oldValue, string.Empty);
    }

    public string CategoryForAggregate<T>()
        where T : Aggregate
    {
        return _cachedTypes.GetOrAdd(typeof(T), _ => typeof(T).Name.ToSnakeCase());
    }
}
