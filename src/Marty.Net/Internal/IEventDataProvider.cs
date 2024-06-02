namespace Marty.Net.Internal;

using System;
using global::EventStore.Client;

internal interface IEventDataProvider
{
    Type EventTypeFrom(ResolvedEvent @event);
}
