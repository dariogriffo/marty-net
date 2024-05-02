namespace Marty.Net.Internal;

using Contracts;
using global::EventStore.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

internal interface IInternalPersistentSubscriber
{
    Task Subscribe(
        string streamName,
        SubscriptionPosition subscriptionPosition,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> onEventAppeared,
        CancellationToken cancellationToken
    );
}
