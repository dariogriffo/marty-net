namespace Marty.Net.Tests;

using Contracts;

internal class TestStreamResolver : IEventsStreamResolver
{
    private readonly string _streamName;

    public TestStreamResolver(string streamName)
    {
        _streamName = streamName;
    }

    public string StreamForEvent<T>(T @event)
        where T : IEvent
    {
        return _streamName;
    }
}
