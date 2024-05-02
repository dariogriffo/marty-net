namespace Marty.Net.Contracts;

using Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///     Configuration for Marty.Net
/// </summary>
public class MartyConfiguration
{
    internal readonly List<ServiceDescriptor> PipelineBehaviorsToRegister = new();
    internal readonly List<ServiceDescriptor> PreProcessorsToRegister = new();
    internal readonly List<ServiceDescriptor> PostProcessorsToRegister = new();
    internal readonly List<ServiceDescriptor> PreAppendersToRegister = new();
    internal readonly List<ServiceDescriptor> PostAppendersToRegister = new();

    /// <summary>
    ///     If set, the implementation to use for the <see cref="IEventsStreamResolver" />
    ///     Not required if you plan to manually Append events
    ///     via <see cref="IEventStore.Append{T}(string,T,System.Threading.CancellationToken)" />
    /// </summary>
    public Type? EventsStreamResolverType { get; set; }

    /// <summary>
    ///     If set, the implementation to use for the <see cref="IConnectionStrategy" />
    /// </summary>
    public Type? ReconnectionStrategyType { get; set; }

    /// <summary>
    ///     The assemblies to scan for <see cref="IEventHandler{T}" />
    /// </summary>
    public Assembly[]? AssembliesToScanForHandlers { get; set; }

    /// <summary>
    ///     The default life time for auto registered <see cref="IEventHandler{T}" />
    /// </summary>
    public ServiceLifetime DefaultHandlesLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    ///     Configure if the events don't have a registered handler to log a warning message and Park them
    /// </summary>
    public bool TreatMissingHandlersAsErrors { get; set; } = false;

    /// <summary>
    ///     Configure if non-marty events that appear while processing a subscription
    ///     will log a warning message and Park them.
    ///     If you subscribe to all you might want this to be false
    /// </summary>
    public bool TreatNonMartyEventsErrors { get; set; } = false;

    /// <summary>
    ///     The options for the Serialization.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } =
        new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceLifetime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public MartyConfiguration AddPostProcessor<T>(
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    ) =>
        RegisterType(
            typeof(T),
            typeof(IPostProcessor<>),
            PostProcessorsToRegister,
            serviceLifetime
        );

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MartyConfiguration AddOpenPostProcessor(
        Type type,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        Type interfaceType = typeof(IPostProcessor<>);
        string? interfaceFullName = interfaceType.FullName;
        return RegisterGeneric(
            type,
            interfaceType,
            PostProcessorsToRegister,
            serviceLifetime,
            interfaceFullName
        );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceLifetime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public MartyConfiguration AddPreProcessor<T>(
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    ) => RegisterType(typeof(T), typeof(IPreProcessor<>), PreProcessorsToRegister, serviceLifetime);

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MartyConfiguration AddOpenPreProcessor(
        Type type,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        Type interfaceType = typeof(IPreProcessor<>);
        string? interfaceFullName = interfaceType.FullName;
        return RegisterGeneric(
            type,
            interfaceType,
            PreProcessorsToRegister,
            serviceLifetime,
            interfaceFullName
        );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceLifetime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public MartyConfiguration AddBehavior<T>(
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    ) =>
        RegisterType(
            typeof(T),
            typeof(IPipelineBehavior<>),
            PipelineBehaviorsToRegister,
            serviceLifetime
        );

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MartyConfiguration AddOpenBehavior(
        Type type,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        Type interfaceType = typeof(IPipelineBehavior<>);
        string? interfaceFullName = interfaceType.FullName;
        return RegisterGeneric(
            type,
            interfaceType,
            PipelineBehaviorsToRegister,
            serviceLifetime,
            interfaceFullName
        );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceLifetime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public MartyConfiguration AddPreAppendAction<T>(
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    ) =>
        RegisterType(
            typeof(T),
            typeof(IPreAppendEventAction<>),
            PreAppendersToRegister,
            serviceLifetime
        );

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MartyConfiguration AddOpenPreAppendAction(
        Type type,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        Type interfaceType = typeof(IPreAppendEventAction<>);
        string? interfaceFullName = interfaceType.FullName;
        return RegisterGeneric(
            type,
            interfaceType,
            PreAppendersToRegister,
            serviceLifetime,
            interfaceFullName
        );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceLifetime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public MartyConfiguration AddPostAppendAction<T>(
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    ) =>
        RegisterType(
            typeof(T),
            typeof(IPostAppendEventAction<>),
            PostAppendersToRegister,
            serviceLifetime
        );

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MartyConfiguration AddOpenPostAppendAction(
        Type type,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        Type interfaceType = typeof(IPostAppendEventAction<>);
        string? interfaceFullName = interfaceType.FullName;
        return RegisterGeneric(
            type,
            interfaceType,
            PostAppendersToRegister,
            serviceLifetime,
            interfaceFullName
        );
    }

    private MartyConfiguration RegisterGeneric(
        Type type,
        Type interfaceType,
        List<ServiceDescriptor> descriptors,
        ServiceLifetime serviceLifetime,
        string? interfaceFullName
    )
    {
        if (!type.IsGenericType)
        {
            throw new InvalidOperationException($"{type.Name} must be generic");
        }

        IEnumerable<Type> implementedGenericInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType)
            .Select(i => i.GetGenericTypeDefinition());

        HashSet<Type> implementedInterfaces = new HashSet<Type>(
            implementedGenericInterfaces.Where(i => i == interfaceType)
        );

        if (implementedInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{type.Name} must implement {interfaceFullName}");
        }

        foreach (Type @interface in implementedInterfaces)
        {
            RegisterType(type, @interface, descriptors, serviceLifetime);
        }

        return this;
    }

    private MartyConfiguration RegisterType(
        Type implementationType,
        Type targetType,
        ICollection<ServiceDescriptor> descriptors,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        List<Type> interfaces = implementationType.FindInterfacesThatClose(targetType).ToList();

        if (interfaces.Count == 0)
        {
            throw new InvalidOperationException(
                $"{implementationType.Name} must implement {targetType.FullName}"
            );
        }
        bool isGenericTypeDefinition =
            implementationType.IsGenericType && implementationType.IsGenericTypeDefinition;

        foreach (Type serviceType in interfaces)
        {
            Type service =
                isGenericTypeDefinition
                && serviceType
                    is
                {
                    IsGenericType: true,
                    IsGenericTypeDefinition: false,
                    ContainsGenericParameters: true
                }
                    ? serviceType.GetGenericTypeDefinition()
                    : serviceType;
            descriptors.Add(new ServiceDescriptor(service, implementationType, serviceLifetime));
        }

        return this;
    }
}
