namespace Marty.Net.Internal;

using Contracts;
using System;

#pragma warning disable 1591

public class NoEventsStreamResolver : IEventsStreamResolver
{
    public string StreamForEvent<T>(T @event)
        where T : IEvent
    {
        throw new NotImplementedException();
    }
}

#pragma warning restore 1591
