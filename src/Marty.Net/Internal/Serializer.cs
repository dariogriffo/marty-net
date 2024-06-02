namespace Marty.Net.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Microsoft.Extensions.Options;

internal sealed class Serializer : ISerializer
{
    private readonly ConcurrentDictionary<string, Type> _cachedTypes = new();
    private readonly string _martyVersion;
    private readonly JsonSerializerOptions _options;

    public Serializer(IOptions<MartyConfiguration> options)
    {
        _options = options.Value.JsonSerializerOptions;
        _martyVersion = GetType()
            .Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!
            .Version;
    }

    public bool Deserialize(
        EventRecord record,
        out IEvent? @event,
        out IDictionary<string, string>? metadata
    )
    {
        @event = null;
        metadata = null;
        Dictionary<string, string>? eventMetadata;
        try
        {
            metadata = eventMetadata = JsonSerializer.Deserialize<Dictionary<string, string>>(
                record.Metadata.Span,
                _options
            );
        }
        catch (Exception)
        {
            return false;
        }

        if (metadata is null)
        {
            return false;
        }

        if (!metadata.ContainsKey(MetadataHeaders.MartyVersion))
        {
            return false;
        }

        ReadOnlyMemory<byte> eventData = record.Data;

        Type type = _cachedTypes.GetOrAdd(
            eventMetadata![MetadataHeaders.AssemblyQualifiedName],
            _ =>
            {
                Type? type1 = Type.GetType(eventMetadata[MetadataHeaders.AssemblyQualifiedName]);
                if (type1 is null)
                {
                    string eventFullName = eventMetadata[MetadataHeaders.EventFullName];
                    string eventAssembly = eventMetadata[MetadataHeaders.AssemblyName];
                    try
                    {
                        Assembly assembly = Assembly.Load(eventAssembly);
                        type1 = assembly.GetType(eventFullName);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (type1 is null)
                {
                    throw new UnknownEventAppeared(record.EventStreamId, record.EventId.ToString());
                }

                return type1;
            }
        );

        @event = JsonSerializer.Deserialize(eventData.Span, type, _options) as IEvent;
        int nonMartyKeys = metadata.Count(x => !x.Key.StartsWith(MetadataHeaders.MartyPrefix));

        if (nonMartyKeys == 0)
        {
            if (metadata.ContainsKey(MetadataHeaders.EmptyMetadata))
            {
                metadata = new Dictionary<string, string>();
            }

            return true;
        }

        {
            FrozenDictionary<string, string> dictionary = metadata
                .Where(x => !x.Key.StartsWith(MetadataHeaders.MartyPrefix))
                .ToFrozenDictionary(x => x.Key, x => x.Value);
            metadata = new Dictionary<string, string>(dictionary);
        }

        return true;
    }

    public EventData Serialize<T>(WriteEnvelope<T> envelope)
        where T : IEvent
    {
        IEvent @event = envelope.Event;
        Type eventType = @event.GetType();

        IDictionary<string, string>? eventMetadata = envelope.Metadata;

        Dictionary<string, string> metadata =
            new(eventMetadata?.Count + 5 ?? 5)
            {
                { MetadataHeaders.MartyVersion, _martyVersion },
                { MetadataHeaders.EventType, eventType.Name },
                { MetadataHeaders.EventFullName, eventType.FullName! },
                { MetadataHeaders.AssemblyName, eventType.Assembly.GetName().Name! },
                { MetadataHeaders.AssemblyQualifiedName, eventType.AssemblyQualifiedName! }
            };

        bool emptyMetadata = false;
        if (eventMetadata is not null)
        {
            emptyMetadata = eventMetadata.Any() == false;
            foreach ((string key, string value) in eventMetadata)
            {
                metadata.Add(key, value);
            }
        }

        if (emptyMetadata)
        {
            metadata.Add(MetadataHeaders.EmptyMetadata, "true");
        }

        byte[] eventBytes = JsonSerializer
            .Serialize(@event, @event.GetType(), _options)
            .ToUtf8Bytes();
        byte[] metadataBytes = JsonSerializer.Serialize(metadata, _options).ToUtf8Bytes();
        return new EventData(Uuid.FromGuid(@event.Id), eventType.Name, eventBytes, metadataBytes);
    }

    private static class MetadataHeaders
    {
        internal static readonly string MartyPrefix = "marty.net";
        internal static readonly string EventType = "marty.net.event.type";
        internal static readonly string AssemblyQualifiedName = "marty.net.event.aqn";
        internal static readonly string AssemblyName = "marty.net.event.assembly.name";
        internal static readonly string EventFullName = "marty.net.event.full.name";
        internal static readonly string EmptyMetadata = "marty.net.empty.metadata";
        internal static readonly string MartyVersion = "marty.net.version";
    }
}
