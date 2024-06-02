namespace Marty.Net.Internal;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using global::EventStore.Client;

internal interface IInternalPersistentSubscriber
{
    Task Subscribe(
        string streamName,
        SubscriptionPosition subscriptionPosition,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> onEventAppeared,
        CancellationToken cancellationToken
    );
}
