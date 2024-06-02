namespace Marty.Net.Internal;

using System.Collections.Generic;
using Contracts;
using global::EventStore.Client;

internal interface ISerializer
{
    bool Deserialize(
        EventRecord record,
        out IEvent? @event,
        out IDictionary<string, string>? metadata
    );

    EventData Serialize<T>(WriteEnvelope<T> envelope)
        where T : IEvent;
}
