namespace Marty.Net.Internal;

using global::EventStore.Client;
using System;

internal interface IEventDataProvider
{
    Type EventTypeFrom(ResolvedEvent @event);
}
