namespace Marty.Net.Aggregates.Contracts;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Net.Contracts;
using ReflectionMagic;

/// <summary>
///     A base Aggregate NOT thread safe
/// </summary>
public abstract class Aggregate
{
    private readonly List<IEvent> _events = [];
    private readonly dynamic _dynamicThis;

    /// <summary>
    /// Constructor
    /// </summary>
    protected Aggregate()
    {
        _dynamicThis = this.AsDynamic();
    }

    /// <summary>
    ///     The id of the Aggregate Root
    /// </summary>
    public string Id { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     All the events that haven't been saved to the store
    /// </summary>
    [JsonIgnore]
    protected internal IEvent[] UncommittedChanges => _events.ToArray();

    /// <summary>
    ///     The Version of the aggregate
    /// </summary>
    public long Version { get; internal set; } = -1;

    /// <summary>
    ///     Clear the uncommitted changes
    /// </summary>
    internal void MarkChangesAsCommitted()
    {
        _events.Clear();
    }

    /// <summary>
    ///     Loads an Aggregate Root to its last know state from the history
    /// </summary>
    /// <param name="history">The history of events.</param>
    public void LoadFromHistory(IEnumerable<ReadEnvelope> history)
    {
        foreach (ReadEnvelope e in history)
        {
            ApplyChange(e.Event, false);
        }
    }

    /// <summary>
    ///     Loads the Aggregate history from the <see cref="Marty.Net.Contracts.IEventStore" />
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
    public async Task Load(
        IAggregateStore eventStore,
        CancellationToken cancellationToken = default
    )
    {
        await eventStore.Hydrate(this, cancellationToken);
    }

    /// <summary>
    ///     Saves the Aggregate history into the <see cref="Marty.Net.Contracts.IEventStore" />
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
    /// <returns>The <see cref="Task" />.</returns>
    public async Task Create(
        IAggregateStore eventStore,
        CancellationToken cancellationToken = default
    )
    {
        await eventStore.Create(this, cancellationToken);
    }

    /// <summary>
    ///     Saves the Aggregate history into the <see cref="Marty.Net.Contracts.IEventStore" />
    /// </summary>
    /// <param name="eventStore">The event store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
    /// <returns>The <see cref="Task" />.</returns>
    public async Task Update(
        IAggregateStore eventStore,
        CancellationToken cancellationToken = default
    )
    {
        await eventStore.Update(this, cancellationToken);
    }

    /// <summary>
    ///     Applies a new change of state to the aggregate root.
    ///     Adding the event to the <see cref="UncommittedChanges" /> list.
    /// </summary>
    /// <param name="event">.</param>
    protected void ApplyChange(IEvent @event)
    {
        ApplyChange(@event, true);
    }

    /// <summary>
    ///     Applies a new change of state to the aggregate root.
    ///     Adding the event to the <see cref="UncommittedChanges" /> list if isNew.
    /// </summary>
    /// <param name="event">.</param>
    /// <param name="isNew">.</param>
    private void ApplyChange(IEvent @event, bool isNew)
    {
        _dynamicThis.Apply(@event);
        ++Version;
        if (isNew)
        {
            _events.Add(@event);
        }
    }
}
