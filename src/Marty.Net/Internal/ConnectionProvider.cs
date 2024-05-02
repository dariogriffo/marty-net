namespace Marty.Net.Internal;

using Contracts;
using global::EventStore.Client;
using System;
using System.Threading.Tasks;

internal sealed class ConnectionProvider : IConnectionProvider, IAsyncDisposable
{
    private readonly EventStoreSettings _settings;
    private Lazy<EventStorePersistentSubscriptionsClient> _persistent;
    private Lazy<EventStoreClient> _read;
    private Lazy<EventStoreClient> _write;
    private volatile bool _stopped = false;

    public ConnectionProvider(EventStoreSettings settings)
    {
        _settings = settings;
        _read = new Lazy<EventStoreClient>(ClientFactory);
        _write = new Lazy<EventStoreClient>(ClientFactory);
        _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(
            PersistentConnectionFactory
        );
    }

    public ValueTask DisposeAsync()
    {
        return StopConnections();
    }

    public EventStorePersistentSubscriptionsClient PersistentSubscriptionClient =>
        _persistent.Value;

    public EventStoreClient ReadClient => _read.Value;

    public EventStoreClient WriteClient => _write.Value;

    public ValueTask ReadClientDisconnected(EventStoreClient client) =>
        IfNotStoppedDisposeAndCreate(
            client,
            () =>
            {
                _read = new Lazy<EventStoreClient>(ClientFactory);
            }
        );

    public ValueTask WriteClientDisconnected(EventStoreClient client) =>
        IfNotStoppedDisposeAndCreate(
            client,
            () =>
            {
                _write = new Lazy<EventStoreClient>(ClientFactory);
            }
        );

    public ValueTask PersistentSubscriptionDisconnected(
        EventStorePersistentSubscriptionsClient client
    ) =>
        IfNotStoppedDisposeAndCreate(
            client,
            () =>
            {
                _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(
                    PersistentConnectionFactory
                );
            }
        );

    public async ValueTask StopConnections()
    {
        lock (_settings)
        {
            if (_stopped)
            {
                return;
            }

            _stopped = true;
        }

        Console.WriteLine("Disposing");

        if (_persistent.IsValueCreated)
        {
            await _persistent.Value.DisposeAsync();
        }

        if (_read.IsValueCreated)
        {
            await _read.Value.DisposeAsync();
        }

        if (_write.IsValueCreated)
        {
            await _write.Value.DisposeAsync();
        }
    }

    private EventStoreClient ClientFactory()
    {
        EventStoreClientSettings settings = EventStoreClientSettings.Create(
            _settings.ConnectionString
        );
        return new EventStoreClient(settings);
    }

    private EventStorePersistentSubscriptionsClient PersistentConnectionFactory()
    {
        EventStoreClientSettings settings = EventStoreClientSettings.Create(
            _settings.ConnectionString
        );
        return new EventStorePersistentSubscriptionsClient(settings);
    }

    private async ValueTask IfNotStoppedDisposeAndCreate(
        IAsyncDisposable disposable,
        Action createFunc
    )
    {
        try
        {
            if (!_stopped)
            {
                await disposable.DisposeAsync();
            }
        }
        catch
        {
            // ignored
        }

        if (!_stopped)
        {
            createFunc();
        }
    }
}
