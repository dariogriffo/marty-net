namespace Marty.Net.Tests;

using System.Collections.Generic;

public interface IStreamNames
{
    public IReadOnlyCollection<string> Streams { get; }
    void Add(string streamName);

    int Count();
}

internal class StreamNames : IStreamNames
{
    private readonly List<string> _streams = [];

    public void Add(string streamName)
    {
        _streams.Add(streamName);
    }

    public int Count()
    {
        return _streams.Count;
    }

    public IReadOnlyCollection<string> Streams => _streams;
}
