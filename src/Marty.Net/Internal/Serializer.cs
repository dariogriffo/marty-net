namespace Marty.Net.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public IEvent? Deserialize(EventRecord record)
    {
        Dictionary<string, string>? metadata;
        try
        {
            metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(
                record.Metadata.Span,
                _options
            );
        }
        catch (Exception)
        {
            return null;
        }

        if (metadata is null)
        {
            return null;
        }

        if (!metadata.ContainsKey(MetadataHeaders.MartyVersion))
        {
            return null;
        }

        ReadOnlyMemory<byte> eventData = record.Data;

        Type type = _cachedTypes.GetOrAdd(
            metadata[MetadataHeaders.AssemblyQualifiedName],
            _ =>
            {
                Type? type1 = Type.GetType(metadata[MetadataHeaders.AssemblyQualifiedName]);
                if (type1 is null)
                {
                    string eventFullName = metadata[MetadataHeaders.EventFullName];
                    string eventAssembly = metadata[MetadataHeaders.AssemblyName];
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

        IEvent? @event = JsonSerializer.Deserialize(eventData.Span, type, _options) as IEvent;
        int nonMartyKeys = metadata.Count(x => !x.Key.StartsWith(MetadataHeaders.MartyPrefix));

        if (nonMartyKeys == 0)
        {
            if (metadata.ContainsKey(MetadataHeaders.EmptyMetadata))
            {
                @event!.Metadata = new Dictionary<string, string>();
            }

            return @event!;
        }

        {
            Dictionary<string, string> dictionary = metadata
                .Where(x => !x.Key.StartsWith(MetadataHeaders.MartyPrefix))
                .ToDictionary(x => x.Key, x => x.Value);
            @event!.Metadata = new ReadOnlyDictionary<string, string>(dictionary);
        }

        return @event;
    }

    public EventData Serialize<T>(T @event)
        where T : IEvent
    {
        Type eventType = @event.GetType();

        IReadOnlyDictionary<string, string>? eventMetadata = @event.Metadata;

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

            @event.Metadata = null;
        }

        if (emptyMetadata)
        {
            metadata.Add(MetadataHeaders.EmptyMetadata, "true");
        }

        byte[] eventBytes = JsonSerializer
            .Serialize(@event, @event.GetType(), _options)
            .ToUtf8Bytes();
        byte[] metadataBytes = JsonSerializer.Serialize(metadata, _options).ToUtf8Bytes();
        return new EventData(
            Uuid.FromGuid(Guid.NewGuid()),
            eventType.Name,
            eventBytes,
            metadataBytes
        );
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
